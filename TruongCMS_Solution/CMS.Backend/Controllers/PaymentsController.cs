// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: Payment Gateway VNPAY (muc 3)
//  - Test case 10: khach bam HUY o trang ngan hang -> ve checkout, GIU NGUYEN gio
//  - Test case 11: khach tat trinh duyet sau khi tra tien -> IPN van cap nhat don
//    (idempotent: IPN ban trung lap khong xu ly 2 lan; co kiem tra CHU KY HMAC)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using Hangfire;

namespace CMS.Backend.Controllers
{
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnPayService _vnPay;
        private readonly IBackgroundJobClient _jobs;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            ApplicationDbContext context,
            IVnPayService vnPay,
            IBackgroundJobClient jobs,
            IConfiguration config,
            ILogger<PaymentsController> logger)
        {
            _context = context;
            _vnPay = vnPay;
            _jobs = jobs;
            _config = config;
            _logger = logger;
        }

        private string FrontendUrl => (_config["FrontendUrl"] ?? "http://localhost:5173").TrimEnd('/');

        /// <summary>
        /// POST /api/payments/create — Frontend xin URL thanh toan cho don VNPAY.
        /// Moi lan goi sinh 1 TxnRef MOI (don co the thanh toan lai sau khi huy ngang).
        /// </summary>
        [HttpPost("api/payments/create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDTO input)
        {
            var order = await _context.Orders.FindAsync(input.OrderId);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (order.PaymentStatus == 1)
                return BadRequest(new { message = "Đơn hàng này đã được thanh toán rồi." });

            if (order.Status != OrderStatusFlow.ChoXacNhan)
                return BadRequest(new { message = "Đơn hàng không còn ở trạng thái chờ thanh toán." });

            // TxnRef duy nhat: Ma don + thoi diem — phan biet cac lan bam thanh toan
            string txnRef = $"{order.Id}-{DateTime.Now:yyyyMMddHHmmssfff}";

            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                OrderId  = order.Id,
                Provider = "VNPAY",
                TxnRef   = txnRef,
                Amount   = order.TotalAmount,
                Status   = 0 // dang cho ket qua
            });
            await _context.SaveChangesAsync();

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            string url = _vnPay.CreatePaymentUrl(txnRef, order.TotalAmount,
                $"Thanh toan don hang #{order.Id} - SHOP.CO", ip);

            return Ok(new { paymentUrl = url, txnRef, demoMode = _vnPay.IsDemoMode });
        }

        /// <summary>
        /// GET /api/payments/vnpay-return — TRINH DUYET KHACH quay ve tu cong thanh toan.
        /// Chi dung de DIEU HUONG giao dien (test case 10); nguon su that la IPN.
        /// Huy/that bai -> ve trang ket qua voi status=cancelled, Frontend GIU NGUYEN GIO HANG.
        /// </summary>
        [HttpGet("api/payments/vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

            if (!_vnPay.ValidateSignature(query))
            {
                _logger.LogWarning("[VNPAY-RETURN] CHU KY KHONG HOP LE: {Query}", Request.QueryString);
                return Redirect($"{FrontendUrl}/payment-result?status=invalid");
            }

            string txnRef = query.GetValueOrDefault("vnp_TxnRef", "");
            string responseCode = query.GetValueOrDefault("vnp_ResponseCode", "");
            var giaoDich = await _context.PaymentTransactions
                .FirstOrDefaultAsync(t => t.TxnRef == txnRef);

            int orderId = giaoDich?.OrderId ?? 0;

            // "00" = thanh cong; "24" = khach bam Huy — cac ma khac coi nhu that bai
            string status = responseCode == "00" ? "success"
                          : responseCode == "24" ? "cancelled"
                          : "failed";

            // Return URL chi DIEU HUONG — viec cap nhat don do IPN dam nhiem.
            // (Truong hop demo/sandbox khong co IPN that thi IPN duoc goi truoc do
            //  boi trang demo, hoac xu ly du phong ngay tai day cho chac chan.)
            if (status == "success" && giaoDich != null && giaoDich.Status == 0)
            {
                await XuLyKetQuaThanhToanAsync(giaoDich, responseCode,
                    query.GetValueOrDefault("vnp_TransactionNo"), Request.QueryString.Value);
            }

            return Redirect($"{FrontendUrl}/payment-result?status={status}&orderId={orderId}");
        }

        /// <summary>
        /// GET /api/payments/vnpay-ipn — WEBHOOK NGAN HANG goi server-to-server
        /// (test case 11: khach tat trinh duyet van nhan duoc ket qua).
        /// IDEMPOTENT: giao dich da xu ly roi -> tra "02 Order already confirmed",
        /// KHONG cap nhat don / gui email lan 2. Co kiem tra chu ky + so tien.
        /// </summary>
        [HttpGet("api/payments/vnpay-ipn")]
        public async Task<IActionResult> VnPayIpn()
        {
            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());

            // 1. CHU KY HMAC-SHA512 phai hop le (chong gia mao IPN — bao cao muc bao mat)
            if (!_vnPay.ValidateSignature(query))
            {
                _logger.LogWarning("[VNPAY-IPN] Chu ky sai: {Query}", Request.QueryString);
                return Ok(new { RspCode = "97", Message = "Invalid signature" });
            }

            string txnRef = query.GetValueOrDefault("vnp_TxnRef", "");
            var giaoDich = await _context.PaymentTransactions
                .Include(t => t.Order)
                .FirstOrDefaultAsync(t => t.TxnRef == txnRef);

            if (giaoDich == null)
                return Ok(new { RspCode = "01", Message = "Order not found" });

            // 2. DOI CHIEU SO TIEN (vnp_Amount nhan 100) — chong sua gia tren duong truyen
            if (long.TryParse(query.GetValueOrDefault("vnp_Amount", "0"), out long amount)
                && amount != (long)(giaoDich.Amount * 100))
            {
                return Ok(new { RspCode = "04", Message = "Invalid amount" });
            }

            // 3. IDEMPOTENT — IPN den TRUNG LAP (ngan hang retry): da xu ly thi bo qua
            if (giaoDich.Status != 0)
            {
                _logger.LogInformation("[VNPAY-IPN] TxnRef {TxnRef} da xu ly truoc do — bo qua (idempotent)", txnRef);
                return Ok(new { RspCode = "02", Message = "Order already confirmed" });
            }

            string responseCode = query.GetValueOrDefault("vnp_ResponseCode", "");
            await XuLyKetQuaThanhToanAsync(giaoDich, responseCode,
                query.GetValueOrDefault("vnp_TransactionNo"), Request.QueryString.Value);

            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }

        /// <summary>
        /// Cap nhat giao dich + don hang DUNG MOT LAN; thanh cong thi gui email
        /// "da thanh toan" bang Hangfire (test case 11).
        /// </summary>
        private async Task XuLyKetQuaThanhToanAsync(
            PaymentTransaction giaoDich, string responseCode, string? gatewayNo, string? rawQuery)
        {
            giaoDich.ResponseCode = responseCode;
            giaoDich.GatewayTransactionNo = gatewayNo;
            giaoDich.ProcessedDate = DateTime.Now;
            giaoDich.RawData = rawQuery;

            if (responseCode == "00")
            {
                giaoDich.Status = 1; // thanh cong
                var order = giaoDich.Order ?? await _context.Orders.FindAsync(giaoDich.OrderId);
                if (order != null && order.PaymentStatus == 0)
                {
                    order.PaymentStatus = 1;
                    order.PaidDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();

                // Email "da nhan tien" chay nen — khong block IPN response
                _jobs.Enqueue<BackgroundJobs>(j => j.GuiEmailThanhToanThanhCong(giaoDich.OrderId));
                _logger.LogInformation("[PAYMENT] Don #{OrderId} DA THANH TOAN ({TxnRef})",
                    giaoDich.OrderId, giaoDich.TxnRef);
            }
            else
            {
                giaoDich.Status = 2; // that bai / khach huy
                await _context.SaveChangesAsync();
                _logger.LogInformation("[PAYMENT] Giao dich {TxnRef} that bai/huy (ma {Code})",
                    giaoDich.TxnRef, responseCode);
            }
        }

        // ═════════════════ TRANG NGAN HANG GIA LAP (DEMO MODE) ═════════════════

        /// <summary>
        /// GET /payment-demo — trang "cong thanh toan gia lap" khi chua co tai khoan
        /// merchant VNPAY that. Khach thay 3 nut:
        ///   [Thanh toan thanh cong]  -> gui IPN + quay ve return URL (luong day du)
        ///   [Huy giao dich]          -> quay ve return URL voi ma 24 (test case 10)
        ///   [Tat trinh duyet sau khi tra tien] -> CHI gui IPN, khong quay ve
        ///                               (giai lap test case 11 — don van duoc cap nhat)
        /// </summary>
        [HttpGet("/payment-demo")]
        public IActionResult PaymentDemo()
        {
            var query = Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
            if (!_vnPay.ValidateSignature(query))
                return Content("<h3>Chữ ký không hợp lệ — không thể mở trang thanh toán.</h3>", "text/html; charset=utf-8");

            string txnRef  = query.GetValueOrDefault("vnp_TxnRef", "");
            string amount  = query.GetValueOrDefault("vnp_Amount", "0");
            string info    = query.GetValueOrDefault("vnp_OrderInfo", "");
            decimal soTien = long.TryParse(amount, out var a) ? a / 100m : 0;

            string html = $@"<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'>
<meta name='viewport' content='width=device-width,initial-scale=1'>
<title>Cổng thanh toán demo - VNPAY Sandbox</title>
<style>
  body{{font-family:'Segoe UI',Arial,sans-serif;background:#f0f2f5;display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0}}
  .card{{background:#fff;border-radius:12px;box-shadow:0 4px 24px rgba(0,0,0,.1);padding:36px;max-width:430px;width:92%}}
  h2{{color:#004a9c;margin:0 0 4px}} .sub{{color:#888;font-size:13px;margin-bottom:20px}}
  .row{{display:flex;justify-content:space-between;padding:9px 0;border-bottom:1px dashed #eee;font-size:14px}}
  .amt{{font-size:26px;color:#d32f2f;font-weight:700;text-align:center;margin:18px 0}}
  button{{width:100%;padding:13px;border:0;border-radius:8px;font-size:15px;cursor:pointer;margin-top:10px;font-weight:600}}
  .ok{{background:#1976d2;color:#fff}} .cancel{{background:#eee;color:#333}}
  .ghost{{background:#fff;border:1px dashed #999;color:#555;font-size:13px}}
  .note{{font-size:12px;color:#999;margin-top:14px;text-align:center}}
</style></head><body>
<div class='card'>
  <h2>VNPAY <small style='font-size:12px;color:#e53935'>DEMO SANDBOX</small></h2>
  <div class='sub'>Trang ngân hàng giả lập — không có giao dịch thật</div>
  <div class='row'><span>Nội dung</span><b>{System.Net.WebUtility.HtmlEncode(info)}</b></div>
  <div class='row'><span>Mã giao dịch</span><b>{System.Net.WebUtility.HtmlEncode(txnRef)}</b></div>
  <div class='amt'>{soTien:N0} ₫</div>
  <button class='ok'     onclick=""go('success', false)"">✓ Thanh toán thành công</button>
  <button class='cancel' onclick=""go('cancel', false)"">✗ Hủy giao dịch / Quay lại</button>
  <button class='ghost'  onclick=""go('success', true)"">⚡ Giả lập: trả tiền xong TẮT TRÌNH DUYỆT (chỉ gửi IPN)</button>
  <div class='note'>Nút thứ 3 minh họa test case 11: trình duyệt không quay về web,
  nhưng webhook IPN vẫn cập nhật đơn hàng &amp; gửi email.</div>
</div>
<script>
  function go(result, ipnOnly) {{
    window.location.href = '/api/payments/demo-complete?txnRef={Uri.EscapeDataString(txnRef)}&result='
      + result + '&ipnOnly=' + ipnOnly;
  }}
</script>
</body></html>";
            return Content(html, "text/html; charset=utf-8");
        }

        /// <summary>
        /// GET /api/payments/demo-complete — trang demo bam nut xong goi ve day.
        /// Mo phong DUNG hanh vi cong that: (1) ngan hang ban IPN server-to-server
        /// (o day la goi noi bo cung logic, kem CHU KY hop le); (2) dua trinh duyet
        /// khach quay ve return URL. ipnOnly=true -> bo qua buoc (2).
        /// </summary>
        [HttpGet("api/payments/demo-complete")]
        public async Task<IActionResult> DemoComplete(
            [FromQuery] string txnRef, [FromQuery] string result, [FromQuery] bool ipnOnly = false)
        {
            var giaoDich = await _context.PaymentTransactions
                .Include(t => t.Order)
                .FirstOrDefaultAsync(t => t.TxnRef == txnRef);

            if (giaoDich == null)
                return NotFound(new { message = "Không tìm thấy giao dịch demo." });

            string responseCode = result == "success" ? "00" : "24"; // 24 = khach huy
            string transactionNo = $"DEMO{DateTime.Now:HHmmssfff}";

            // (1) GIA LAP IPN: ky bo tham so nhu ngan hang that roi xu ly cung logic
            var ipnParams = new Dictionary<string, string>
            {
                ["vnp_TxnRef"]        = txnRef,
                ["vnp_Amount"]        = ((long)(giaoDich.Amount * 100)).ToString(),
                ["vnp_ResponseCode"]  = responseCode,
                ["vnp_TransactionNo"] = transactionNo,
                ["vnp_PayDate"]       = DateTime.Now.ToString("yyyyMMddHHmmss"),
            };
            string chuKy = _vnPay.Sign(ipnParams);
            string rawIpn = string.Join("&", ipnParams.Select(p => $"{p.Key}={p.Value}")) + $"&vnp_SecureHash={chuKy}";

            // Xu ly IPN (idempotent — bam nut 2 lan cung khong cap nhat trung)
            if (giaoDich.Status == 0)
            {
                await XuLyKetQuaThanhToanAsync(giaoDich, responseCode, transactionNo, rawIpn);
            }

            if (ipnOnly)
            {
                // Khach "tat trinh duyet": chi co IPN chay — hien trang xac nhan cho nguoi cham bai
                return Content($@"<html lang='vi'><head><meta charset='utf-8'><title>IPN demo</title></head>
<body style='font-family:Segoe UI,Arial;max-width:560px;margin:60px auto'>
<h2>⚡ Đã giả lập: khách tắt trình duyệt</h2>
<p>Trình duyệt KHÔNG quay về website, nhưng webhook <b>IPN</b> đã được gửi server-to-server:</p>
<ul><li>Giao dịch: <b>{txnRef}</b> — kết quả: <b>{(responseCode == "00" ? "THÀNH CÔNG" : "HỦY")}</b></li>
<li>Đơn hàng #{giaoDich.OrderId} đã được cập nhật trạng thái thanh toán</li>
<li>Email xác nhận đã được đưa vào hàng đợi Hangfire</li></ul>
<p>Mở trang <a href='{FrontendUrl}/orders'>Đơn hàng của tôi</a> để thấy đơn đã ghi nhận ""Đã thanh toán"".</p>
</body></html>", "text/html; charset=utf-8");
            }

            // (2) Dua khach quay ve return URL nhu cong that (kem chu ky hop le)
            var returnParams = new Dictionary<string, string>(ipnParams);
            string returnHash = _vnPay.Sign(returnParams);
            string qs = string.Join("&", returnParams.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            return Redirect($"/api/payments/vnpay-return?{qs}&vnp_SecureHash={returnHash}");
        }
    }

    public class CreatePaymentDTO
    {
        public int OrderId { get; set; }
    }
}
