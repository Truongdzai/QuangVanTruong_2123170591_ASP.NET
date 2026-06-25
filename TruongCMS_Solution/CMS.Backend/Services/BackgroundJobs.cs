// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: tac vu nen Hangfire (muc 7 — Background Jobs)

using System.Text;
using CMS.Data;
using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services;

/// <summary>
/// CAC TAC VU NEN chay bang HANGFIRE (muc 7 bao cao — System &amp; Operations):
///  1. HuyDonQuaHanThanhToan  — RecurringJob 5 phut/lan: don VNPAY qua 15 phut
///     chua tra tien -> tu huy + HOAN KHO (giai phong hang da "reserve")
///  2. TatMaGiamGiaHetHan     — RecurringJob hang ngay: tat IsActive cac ma het han
///  3. GuiEmailXacNhanDon     — BackgroundJob.Enqueue: gui email NGOAI luong dat hang
///     (test case 13: bam dat hang xong la job kich hoat SMTP, khong bat khach cho)
/// </summary>
public class BackgroundJobs
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderWorkflowService _workflow;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<BackgroundJobs> _logger;

    public BackgroundJobs(
        ApplicationDbContext context,
        IOrderWorkflowService workflow,
        IEmailService email,
        IConfiguration config,
        ILogger<BackgroundJobs> logger)
    {
        _context = context;
        _workflow = workflow;
        _email = email;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Don thanh toan ONLINE qua TTL (mac dinh 15 phut) van chua tra tien -> tu HUY.
    /// Bao cao muc "Quan ly ton kho & canh tranh": hang da tru kho khi dat
    /// duoc GIAI PHONG cho khach khac mua (chong giu cho vo thoi han).
    /// </summary>
    public async Task HuyDonQuaHanThanhToan()
    {
        int phutQuaHan = int.TryParse(_config["Payment:UnpaidOrderTtlMinutes"], out var m) ? m : 15;
        var hanChot = DateTime.Now.AddMinutes(-phutQuaHan);

        var danhSach = await _context.Orders
            .Where(o => o.PaymentMethod == "VNPAY"
                        && o.PaymentStatus == 0                       // chua thanh toan
                        && o.Status == OrderStatusFlow.ChoXacNhan     // chua ai xu ly
                        && o.OrderDate < hanChot)
            .Select(o => o.Id)
            .ToListAsync();

        foreach (int orderId in danhSach)
        {
            var kq = await _workflow.DoiTrangThaiAsync(orderId, OrderStatusFlow.DaHuy,
                $"Tự động hủy: quá {phutQuaHan} phút chưa thanh toán online");
            _logger.LogInformation("[HANGFIRE] Huy don qua han #{OrderId}: {Ok}", orderId, kq.ThanhCong);
        }
    }

    /// <summary>Ma giam gia het han / het luot -> tat IsActive (don dep moi ngay)</summary>
    public async Task TatMaGiamGiaHetHan()
    {
        var now = DateTime.Now;
        int soTat = await _context.DiscountCodes
            .Where(d => d.IsActive &&
                   ((d.ExpiryDate != null && d.ExpiryDate < now) ||
                    (d.MaxUses > 0 && d.UsedCount >= d.MaxUses)))
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.IsActive, false));

        if (soTat > 0)
            _logger.LogInformation("[HANGFIRE] Da tat {Count} ma giam gia het han/het luot", soTat);
    }

    /// <summary>
    /// Gui email xac nhan don hang (duoc Enqueue ngay khi dat hang thanh cong —
    /// test case 13). Hangfire tu RETRY neu SMTP loi tam thoi.
    /// </summary>
    public async Task GuiEmailXacNhanDon(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order?.Customer == null) return;

        var dong = new StringBuilder();
        decimal tamTinh = 0;
        foreach (var d in order.OrderDetails)
        {
            string tenMon = d.Product?.Name ?? $"Sản phẩm #{d.ProductId}";
            if (!string.IsNullOrEmpty(d.VariantLabel)) tenMon += $" ({d.VariantLabel})";
            tamTinh += d.UnitPrice * d.Quantity;
            dong.Append(
                $"<tr><td style='padding:6px 12px;border:1px solid #ddd'>{tenMon}</td>" +
                $"<td style='padding:6px 12px;border:1px solid #ddd;text-align:center'>{d.Quantity}</td>" +
                $"<td style='padding:6px 12px;border:1px solid #ddd;text-align:right'>{d.UnitPrice:N0} ₫</td></tr>");
        }

        decimal tienGiam = Math.Round(tamTinh * order.DiscountPercent / 100m);
        string dongGiam = order.DiscountPercent > 0
            ? $"<p>Mã giảm giá <b>{order.DiscountCode}</b> (-{order.DiscountPercent}%): <b>-{tienGiam:N0} ₫</b></p>"
            : "";
        string dongShip = order.ShippingFee > 0
            ? $"<p>Phí vận chuyển: {order.ShippingFee:N0} ₫</p>"
            : "<p>Phí vận chuyển: Miễn phí</p>";
        string dongThanhToan = order.PaymentMethod == "VNPAY"
            ? (order.PaymentStatus == 1 ? "<p>Thanh toán online: <b style='color:green'>ĐÃ THANH TOÁN</b></p>"
                                        : "<p>Thanh toán online: <b style='color:#c60'>Chờ thanh toán</b></p>")
            : "<p>Hình thức: Thanh toán khi nhận hàng (COD)</p>";

        string noiDung =
            $"<h2>Cảm ơn {order.Customer.FullName} đã đặt hàng tại SHOP.CO!</h2>" +
            $"<p>Mã đơn hàng: <b>#{order.Id}</b> — Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}</p>" +
            "<table style='border-collapse:collapse'>" +
            "<tr><th style='padding:6px 12px;border:1px solid #ddd'>Sản phẩm</th>" +
            "<th style='padding:6px 12px;border:1px solid #ddd'>SL</th>" +
            "<th style='padding:6px 12px;border:1px solid #ddd'>Đơn giá</th></tr>" +
            dong +
            $"</table><p>Tạm tính: {tamTinh:N0} ₫</p>" +
            dongGiam + dongShip +
            $"<p><b>Thành tiền: {order.TotalAmount:N0} ₫</b></p>" +
            dongThanhToan +
            $"<p>Giao tới: {order.Customer.Address} — SĐT: {order.Customer.Phone}</p>" +
            "<p>Chúng tôi sẽ liên hệ xác nhận trong thời gian sớm nhất.</p>";

        await _email.SendAsync(order.Customer.Email,
            $"SHOP.CO - Xác nhận đơn hàng #{order.Id}", noiDung);
    }

    /// <summary>Email bao "DA THANH TOAN THANH CONG" sau khi IPN xac nhan tien ve (test case 11)</summary>
    public async Task GuiEmailThanhToanThanhCong(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order?.Customer == null) return;

        await _email.SendAsync(order.Customer.Email,
            $"SHOP.CO - Đơn hàng #{order.Id} đã thanh toán thành công",
            $"<h2>Xin chào {order.Customer.FullName},</h2>" +
            $"<p>Chúng tôi đã nhận được <b>{order.TotalAmount:N0} ₫</b> cho đơn hàng <b>#{order.Id}</b> qua {order.PaymentMethod}.</p>" +
            "<p>Đơn hàng sẽ được đóng gói và bàn giao cho đơn vị vận chuyển sớm nhất.</p>");
    }
}
