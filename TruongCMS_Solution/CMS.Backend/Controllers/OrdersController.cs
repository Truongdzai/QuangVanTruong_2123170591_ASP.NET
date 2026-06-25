// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU CHUYEN SAU (deep-research-report):
//  - Bien the SKU trong don hang (test case 1-3)
//  - CHONG RACE CONDITION khi tru kho bang UPDATE nguyen tu (test case 6)
//  - Ma giam gia: don toi thieu + gioi han luot/khach (test case 7-8)
//  - Gia flash sale tinh theo GIO SERVER (test case 9)
//  - Thanh toan COD / VNPAY + phi van chuyen (test case 10-11)
//  - Huy don dung MAY TRANG THAI (test case 12) + email qua Hangfire (test case 13)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using System.ComponentModel.DataAnnotations;
using Hangfire;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricing;
        private readonly IShippingService _shipping;
        private readonly IOrderWorkflowService _workflow;
        private readonly IBackgroundJobClient _jobs;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            ApplicationDbContext context,
            IPricingService pricing,
            IShippingService shipping,
            IOrderWorkflowService workflow,
            IBackgroundJobClient jobs,
            ILogger<OrdersController> logger)
        {
            _context = context;
            _pricing = pricing;
            _shipping = shipping;
            _workflow = workflow;
            _jobs = jobs;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/Orders — "Bam Dat Hang" tu ReactJS:
        ///  1) Tim/tao Customer theo email
        ///  2) Tinh GIA HIEU LUC tung mon o SERVER (flash sale check gio server — test case 9;
        ///     bien the co gia rieng — test case 3). KHONG tin gia Frontend gui len.
        ///  3) TRU KHO NGUYEN TU: UPDATE ... WHERE StockQuantity &gt;= qty — hai khach tranh
        ///     chiec ao cuoi cung thi nguoi bam sau nhan loi "vua het hang" (test case 6)
        ///  4) Kiem tra ma giam gia voi TAM TINH THAT (test case 7) + luot/khach (test case 8)
        ///  5) COD: vao thang quy trinh. VNPAY: tra requiresPayment de Frontend xin URL thanh toan
        ///  6) Gui email xac nhan bang HANGFIRE job — khong bat khach ngoi cho SMTP (test case 13)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderInputDTO input)
        {
            if (input == null || input.Items == null || input.Items.Count == 0)
            {
                return BadRequest(new { message = "Giỏ hàng trống — không thể tạo đơn hàng." });
            }

            string paymentMethod = (input.PaymentMethod ?? "COD").Trim().ToUpper();
            if (paymentMethod != "COD" && paymentMethod != "VNPAY")
            {
                return BadRequest(new { message = "Phương thức thanh toán không hợp lệ (COD hoặc VNPAY)." });
            }

            // Transaction: don + chi tiet + tru kho — hoac tat ca, hoac khong gi ca
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ── Buoc A: Tim / tao khach hang theo email ──────────────────
                string email = input.Email.Trim().ToLower();
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email);

                if (customer == null)
                {
                    customer = new Customer
                    {
                        FullName = input.FullName.Trim(),
                        Email    = email,
                        Phone    = input.Phone,
                        Address  = input.Address,
                        Password = PasswordHasher.Hash(Guid.NewGuid().ToString("N")),
                        IsGuest  = true
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    customer.FullName = input.FullName.Trim();
                    customer.Phone    = input.Phone;
                    customer.Address  = input.Address;
                }

                // ── Buoc B: Duyet gio hang — gia server + tru kho nguyen tu ──
                var bangGiaFlash = await _pricing.GetActiveFlashPricesAsync(); // GIO SERVER
                decimal tamTinh = 0;
                int tongSoMon = 0;
                var chiTietList = new List<OrderDetail>();
                var flashDaBan = new Dictionary<int, int>(); // ProductId -> so luong ban gia sale

                foreach (var item in input.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "Số lượng sản phẩm phải lớn hơn 0." });
                    }

                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = $"Sản phẩm Id={item.ProductId} không tồn tại." });
                    }

                    // Bien the SKU (neu khach chon): phai thuoc dung san pham & con kinh doanh
                    ProductVariant? variant = null;
                    if (item.VariantId.HasValue)
                    {
                        variant = await _context.ProductVariants
                            .FirstOrDefaultAsync(v => v.Id == item.VariantId.Value
                                                   && v.ProductId == product.Id
                                                   && v.IsActive);
                        if (variant == null)
                        {
                            await transaction.RollbackAsync();
                            return BadRequest(new { message = $"Biến thể sản phẩm \"{product.Name}\" không hợp lệ." });
                        }
                    }

                    // ── TEST CASE 6 — TRU KHO NGUYEN TU (chong ban qua ton) ──
                    // UPDATE ... WHERE StockQuantity >= qty: SQL Server khoa dong khi update,
                    // 2 nguoi cung mua chiec cuoi -> nguoi sau affected = 0 -> bao het hang.
                    int affected;
                    if (variant != null)
                    {
                        affected = await _context.Database.ExecuteSqlInterpolatedAsync(
                            $"UPDATE ProductVariants SET StockQuantity = StockQuantity - {item.Quantity} WHERE Id = {variant.Id} AND StockQuantity >= {item.Quantity}");

                        if (affected > 0)
                        {
                            // Dong bo ton kho tong cua san pham (khong de am)
                            await _context.Database.ExecuteSqlInterpolatedAsync(
                                $"UPDATE Products SET StockQuantity = CASE WHEN StockQuantity >= {item.Quantity} THEN StockQuantity - {item.Quantity} ELSE 0 END WHERE Id = {product.Id}");
                        }
                    }
                    else
                    {
                        affected = await _context.Database.ExecuteSqlInterpolatedAsync(
                            $"UPDATE Products SET StockQuantity = StockQuantity - {item.Quantity} WHERE Id = {product.Id} AND StockQuantity >= {item.Quantity}");
                    }

                    if (affected == 0)
                    {
                        await transaction.RollbackAsync();
                        int conLai = variant != null
                            ? await _context.ProductVariants.Where(v => v.Id == variant.Id).Select(v => v.StockQuantity).FirstOrDefaultAsync()
                            : await _context.Products.Where(p => p.Id == product.Id).Select(p => p.StockQuantity).FirstOrDefaultAsync();

                        // 409 Conflict: san pham vua het hang / khong du so luong
                        return Conflict(new
                        {
                            message = conLai <= 0
                                ? $"Rất tiếc, \"{product.Name}\" vừa hết hàng!"
                                : "Số lượng sản phẩm trong kho không đủ!",
                            productId = product.Id,
                            productName = product.Name,
                            variantId = variant?.Id,
                            stockQuantity = conLai
                        });
                    }

                    // ── Gia hieu luc tai SERVER (test case 3 + 9) ────────────
                    decimal giaNiemYet = variant?.PriceOverride ?? product.Price;
                    decimal giaBan = giaNiemYet;
                    if (bangGiaFlash.TryGetValue(product.Id, out var flash) && flash.SalePrice < giaNiemYet)
                    {
                        giaBan = flash.SalePrice;
                        flashDaBan[product.Id] = flashDaBan.GetValueOrDefault(product.Id) + item.Quantity;
                    }

                    chiTietList.Add(new OrderDetail
                    {
                        ProductId        = product.Id,
                        ProductVariantId = variant?.Id,
                        VariantLabel     = variant == null ? null : $"{variant.ColorName ?? variant.Color} / {variant.Size}",
                        Quantity         = item.Quantity,
                        UnitPrice        = giaBan
                    });

                    tamTinh += giaBan * item.Quantity;
                    tongSoMon += item.Quantity;
                }

                // ── Buoc C: Ma giam gia — kiem tra voi TAM TINH THAT (test case 7-8)
                var (maGiam, loiMa) = await DiscountsController.KiemTraMaAsync(
                    _context, input.DiscountCode, email, tamTinh);
                if (!string.IsNullOrWhiteSpace(input.DiscountCode) && maGiam == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = loiMa ?? "Mã giảm giá không hợp lệ." });
                }

                decimal tienGiam = maGiam == null ? 0 : Math.Round(tamTinh * maGiam.Percent / 100m);
                decimal phiShip  = _shipping.TinhPhiShip(tamTinh - tienGiam, tongSoMon);
                decimal thanhTien = tamTinh - tienGiam + phiShip;

                // ── Buoc D: Ghi don hang ─────────────────────────────────────
                var newOrder = new Order
                {
                    OrderDate       = DateTime.Now,
                    CustomerId      = customer.Id,
                    Status          = OrderStatusFlow.ChoXacNhan,
                    Notes           = input.Notes,
                    DiscountCode    = maGiam?.Code,
                    DiscountPercent = maGiam?.Percent ?? 0,
                    PaymentMethod   = paymentMethod,
                    PaymentStatus   = 0,
                    ShippingFee     = phiShip,
                    TotalAmount     = thanhTien
                };
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                foreach (var ct in chiTietList)
                {
                    ct.OrderId = newOrder.Id;
                    _context.OrderDetails.Add(ct);
                }

                // Ghi nhan luot dung ma (test case 8: lan sau nhap lai bi chan)
                if (maGiam != null)
                {
                    maGiam.UsedCount++;
                    _context.DiscountUsages.Add(new DiscountUsage
                    {
                        DiscountCodeId = maGiam.Id,
                        CustomerEmail  = email,
                        OrderId        = newOrder.Id
                    });
                }

                // Cong don suat flash sale da ban
                foreach (var (productId, soLuong) in flashDaBan)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync(
                        $@"UPDATE fi SET fi.SoldCount = fi.SoldCount + {soLuong}
                           FROM FlashSaleItems fi
                           JOIN FlashSales f ON f.Id = fi.FlashSaleId
                           WHERE fi.ProductId = {productId} AND f.IsActive = 1
                             AND f.StartTime <= GETDATE() AND f.EndTime > GETDATE()");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // ── Buoc E: Email xac nhan qua HANGFIRE (test case 13) ───────
                // Job chay NGOAI request — SMTP cham/loi khong anh huong dat hang,
                // Hangfire tu retry neu gui that bai.
                _jobs.Enqueue<BackgroundJobs>(j => j.GuiEmailXacNhanDon(newOrder.Id));

                return StatusCode(201, new
                {
                    message = paymentMethod == "VNPAY"
                        ? "Đơn hàng đã tạo — vui lòng thanh toán online để hoàn tất."
                        : "Đặt hàng thành công!",
                    orderId = newOrder.Id,
                    subtotal = tamTinh,
                    discountCode = maGiam?.Code,
                    discountPercent = maGiam?.Percent ?? 0,
                    discountAmount = tienGiam,
                    shippingFee = phiShip,
                    totalAmount = thanhTien,
                    paymentMethod,
                    // VNPAY: Frontend goi tiep POST /api/payments/create de lay URL cong thanh toan
                    requiresPayment = paymentMethod == "VNPAY",
                    emailSent = true
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Loi tao don hang");
                return StatusCode(500, new { message = "Lỗi xử lý tạo đơn hàng ngầm", detail = ex.Message });
            }
        }

        /// <summary>
        /// GET api/Orders/lookup?email= — trang "Don hang cua toi" (kem trang thai
        /// theo may trang thai moi, thanh toan, ma van don).
        /// </summary>
        [HttpGet("lookup")]
        public async Task<IActionResult> Lookup([FromQuery] string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Vui lòng cung cấp email để tra cứu đơn hàng." });
            }

            string e = email.Trim().ToLower();

            var orders = await _context.Orders
                .Where(o => o.Customer != null && o.Customer.Email.ToLower() == e)
                .OrderByDescending(o => o.Id)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    StatusName = "",          // dien sau bang OrderStatusFlow (tranh dich SQL)
                    o.Notes,
                    o.DiscountCode,
                    o.DiscountPercent,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.ShippingFee,
                    o.TrackingCode,
                    o.ShippingProvider,
                    o.CancelReason,
                    o.LoyaltyPointsEarned,
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.ProductId,
                        ProductName = od.Product != null ? od.Product.Name : "",
                        ImageUrl = od.Product != null ? od.Product.ImageUrl : null,
                        od.VariantLabel,
                        od.Quantity,
                        od.UnitPrice
                    }),
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();

            // Gan ten trang thai tieng Viet (lam o C# vi EF khong dich duoc ham nay sang SQL)
            var ketQua = orders.Select(o => new
            {
                o.Id, o.OrderDate, o.Status,
                StatusName = OrderStatusFlow.TenTrangThai(o.Status),
                o.Notes, o.DiscountCode, o.DiscountPercent,
                o.PaymentMethod, o.PaymentStatus, o.ShippingFee,
                o.TrackingCode, o.ShippingProvider, o.CancelReason,
                o.LoyaltyPointsEarned, o.Items, o.TotalAmount,
                // Khach duoc tu huy khi don con CHO XAC NHAN
                CanCancel = o.Status == OrderStatusFlow.ChoXacNhan
            });

            return Ok(ketQua);
        }

        /// <summary>GET api/Orders/{id} — man hinh xac nhan sau khi dat.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    o.Notes,
                    o.PaymentMethod,
                    o.PaymentStatus,
                    o.ShippingFee,
                    o.TrackingCode,
                    CustomerName = o.Customer != null ? o.Customer.FullName : "",
                    Items = o.OrderDetails.Select(od => new
                    {
                        od.ProductId,
                        ProductName = od.Product != null ? od.Product.Name : "",
                        od.VariantLabel,
                        od.Quantity,
                        od.UnitPrice
                    }),
                    TotalAmount = o.TotalAmount
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng này." });

            return Ok(order);
        }

        /// <summary>
        /// POST api/Orders/{id}/cancel — khach TU HUY don khi con "Cho xac nhan".
        /// Di qua may trang thai (test case 12) -> tu dong HOAN KHO + tra luot ma giam gia.
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderDTO input)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            // Xac minh chu don bang email (khach vang lai khong co tai khoan)
            string email = (input?.Email ?? "").Trim().ToLower();
            if (order.Customer == null || order.Customer.Email.ToLower() != email)
                return StatusCode(403, new { message = "Email không khớp với đơn hàng — không thể hủy." });

            if (order.Status != OrderStatusFlow.ChoXacNhan)
                return BadRequest(new
                {
                    message = $"Đơn đang ở trạng thái \"{OrderStatusFlow.TenTrangThai(order.Status)}\" — " +
                              "chỉ hủy được khi còn \"Chờ xác nhận\". Vui lòng liên hệ CSKH."
                });

            var kq = await _workflow.DoiTrangThaiAsync(id, OrderStatusFlow.DaHuy,
                string.IsNullOrWhiteSpace(input?.Reason) ? "Khách tự hủy đơn" : input!.Reason);

            if (!kq.ThanhCong)
                return BadRequest(new { message = kq.Loi });

            return Ok(new { message = "Đã hủy đơn hàng và hoàn lại tồn kho.", orderId = id });
        }

        /// <summary>
        /// GET api/Orders/{id}/invoice?email= — HOA DON dien tu dang HTML kho A4
        /// (muc 2 bao cao). Mo tab moi -> Ctrl+P de luu PDF.
        /// </summary>
        [HttpGet("{id:int}/invoice")]
        public async Task<IActionResult> Invoice(int id, [FromQuery] string? email)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order?.Customer == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            // Chan xem trom hoa don nguoi khac: phai kem dung email chu don
            if (!string.Equals(order.Customer.Email, (email ?? "").Trim(),
                               StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new { message = "Email không khớp với đơn hàng." });

            decimal tamTinh = order.OrderDetails.Sum(d => d.UnitPrice * d.Quantity);
            decimal tienGiam = Math.Round(tamTinh * order.DiscountPercent / 100m);

            var rows = string.Join("", order.OrderDetails.Select((d, i) =>
                $"<tr><td>{i + 1}</td><td>{d.Product?.Name}" +
                (string.IsNullOrEmpty(d.VariantLabel) ? "" : $" <small>({d.VariantLabel})</small>") +
                $"</td><td class='r'>{d.Quantity}</td><td class='r'>{d.UnitPrice:N0}</td>" +
                $"<td class='r'>{d.UnitPrice * d.Quantity:N0}</td></tr>"));

            string html = $@"<!DOCTYPE html><html lang='vi'><head><meta charset='utf-8'>
<title>Hóa đơn #{order.Id} - SHOP.CO</title>
<style>
  body{{font-family:'Segoe UI',Arial,sans-serif;max-width:760px;margin:24px auto;color:#111}}
  h1{{letter-spacing:2px}} table{{width:100%;border-collapse:collapse;margin-top:16px}}
  th,td{{border:1px solid #ccc;padding:8px 10px;font-size:14px}} th{{background:#f4f4f4}}
  .r{{text-align:right}} .tot td{{font-weight:bold;background:#fafafa}}
  .meta{{display:flex;justify-content:space-between;margin-top:12px;font-size:14px}}
  .badge{{display:inline-block;padding:2px 10px;border-radius:12px;background:#eee;font-size:12px}}
  @media print{{ .no-print{{display:none}} }}
</style></head><body>
<h1>SHOP.CO</h1>
<p><b>HÓA ĐƠN BÁN HÀNG #{order.Id}</b> <span class='badge'>{OrderStatusFlow.TenTrangThai(order.Status)}</span></p>
<div class='meta'>
  <div>
    <b>Khách hàng:</b> {order.Customer.FullName}<br>
    <b>Email:</b> {order.Customer.Email}<br>
    <b>SĐT:</b> {order.Customer.Phone} — <b>Địa chỉ:</b> {order.Customer.Address}
  </div>
  <div>
    <b>Ngày đặt:</b> {order.OrderDate:dd/MM/yyyy HH:mm}<br>
    <b>Thanh toán:</b> {order.PaymentMethod} ({(order.PaymentStatus == 1 ? "Đã thanh toán" : order.PaymentStatus == 2 ? "Đã hoàn tiền" : "Chưa thanh toán")})<br>
    <b>Vận đơn:</b> {order.TrackingCode ?? "—"}
  </div>
</div>
<table>
<tr><th>#</th><th>Sản phẩm</th><th class='r'>SL</th><th class='r'>Đơn giá</th><th class='r'>Thành tiền</th></tr>
{rows}
<tr><td colspan='4' class='r'>Tạm tính</td><td class='r'>{tamTinh:N0}</td></tr>
<tr><td colspan='4' class='r'>Giảm giá {(order.DiscountPercent > 0 ? $"({order.DiscountCode} -{order.DiscountPercent}%)" : "")}</td><td class='r'>-{tienGiam:N0}</td></tr>
<tr><td colspan='4' class='r'>Phí vận chuyển</td><td class='r'>{order.ShippingFee:N0}</td></tr>
<tr class='tot'><td colspan='4' class='r'>TỔNG CỘNG</td><td class='r'>{order.TotalAmount:N0}</td></tr>
</table>
<p style='margin-top:24px;font-size:13px;color:#555'>Cảm ơn quý khách đã mua sắm tại SHOP.CO!</p>
<button class='no-print' onclick='window.print()' style='padding:10px 24px;cursor:pointer'>In / Lưu PDF</button>
</body></html>";

            return Content(html, "text/html; charset=utf-8");
        }
    }

    // ── DTO Frontend gui len ─────────────────────────────────────────────────

    public class OrderInputDTO
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống"), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        public string Address { get; set; } = string.Empty;

        public string? Notes { get; set; }

        /// <summary>Ma giam gia khach ap o gio hang (Backend kiem tra lai voi tam tinh that)</summary>
        public string? DiscountCode { get; set; }

        /// <summary>COD (mac dinh) hoac VNPAY</summary>
        public string? PaymentMethod { get; set; }

        [Required]
        public List<OrderItemDTO> Items { get; set; } = new();
    }

    public class OrderItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        /// <summary>Id bien the SKU khach chon (null = san pham khong co bien the)</summary>
        public int? VariantId { get; set; }
    }

    public class CancelOrderDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }
}
