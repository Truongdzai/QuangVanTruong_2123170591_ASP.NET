// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: kiem tra ma giam gia TREN SERVER —
// don toi thieu (test case 7) + gioi han luot moi khach (test case 8)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace CMS.Backend.Controllers
{
    /// <summary>
    /// API JSON: POST /api/Discounts/validate — gio hang gui ma + TAM TINH len,
    /// he thong kiem tra DAY DU DIEU KIEN tren server (khong tin Frontend):
    /// ton tai / con bat / con han / con luot / dung chu ma / DU DON TOI THIEU /
    /// CHUA VUOT LUOT CUA RIENG KHACH NAY.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiscountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ham dung chung (gio hang validate + dat hang chot don):
        /// kiem tra 1 ma giam gia voi gia tri don cu the. Tra ve (ma hop le, loi).
        /// </summary>
        public static async Task<(DiscountCode? code, string? error)> KiemTraMaAsync(
            ApplicationDbContext context, string? rawCode, string? email, decimal subtotal = 0)
        {
            if (string.IsNullOrWhiteSpace(rawCode)) return (null, null); // khong dung ma

            string ma = rawCode.Trim().ToUpper();
            var code = await context.DiscountCodes.FirstOrDefaultAsync(d => d.Code == ma);

            if (code == null || !code.IsActive)
                return (null, "Mã giảm giá không hợp lệ hoặc đã hết hạn.");

            if (code.ExpiryDate.HasValue && code.ExpiryDate.Value < DateTime.Now)
                return (null, "Mã giảm giá đã hết hạn sử dụng.");

            if (code.MaxUses > 0 && code.UsedCount >= code.MaxUses)
                return (null, "Mã giảm giá đã hết lượt sử dụng.");

            // TEST CASE 7 — DON TOI THIEU: gio 450 nhap ma "tu 500" phai bi tu choi;
            // mua them len 520 thi ap thanh cong. Kiem tra bang TAM TINH SERVER tinh.
            if (code.MinOrderAmount > 0 && subtotal < code.MinOrderAmount)
                return (null, $"Mã {code.Code} chỉ áp dụng cho đơn hàng từ " +
                              $"{code.MinOrderAmount:N0} — đơn của bạn mới đạt {subtotal:N0}. " +
                              $"Mua thêm {(code.MinOrderAmount - subtotal):N0} để dùng mã nhé!");

            string e = (email ?? "").Trim().ToLower();

            // Ma ca nhan (chao mung) -> chi dung voi dung email cua chu ma
            if (code.CustomerId.HasValue)
            {
                var chuMa = await context.Customers.FindAsync(code.CustomerId.Value);
                if (chuMa == null || chuMa.Email.ToLower() != e)
                    return (null, "Mã này là mã cá nhân — vui lòng dùng đúng email đã đăng ký.");
            }

            // TEST CASE 8 — GIOI HAN LUOT MOI KHACH: dem trong bang DiscountUsages;
            // dat don thu 2 voi ma "1 lan/khach" -> bao "Ma da duoc su dung".
            if (code.MaxUsesPerCustomer > 0)
            {
                if (string.IsNullOrEmpty(e))
                    return (null, "Vui lòng nhập email trước khi áp mã giảm giá này.");

                int daDung = await context.DiscountUsages
                    .CountAsync(u => u.DiscountCodeId == code.Id && u.CustomerEmail == e);

                if (daDung >= code.MaxUsesPerCustomer)
                    return (null, $"Mã {code.Code} đã được sử dụng — mỗi khách chỉ được dùng " +
                                  $"{code.MaxUsesPerCustomer} lần.");
            }

            return (code, null);
        }

        /// <summary>POST /api/Discounts/validate — gio hang kiem tra ma truoc khi ap</summary>
        [HttpPost("validate")]
        public async Task<IActionResult> Validate([FromBody] ValidateDiscountDTO input)
        {
            var (code, error) = await KiemTraMaAsync(_context, input.Code, input.Email, input.Subtotal);

            if (code == null)
            {
                return Ok(new { valid = false, message = error ?? "Vui lòng nhập mã giảm giá." });
            }

            decimal discountAmount = Math.Round(input.Subtotal * code.Percent / 100m);

            return Ok(new
            {
                valid = true,
                code = code.Code,
                percent = code.Percent,
                minOrderAmount = code.MinOrderAmount,
                discountAmount,
                message = $"Đã áp dụng mã {code.Code} (-{code.Percent}%)"
            });
        }
    }

    public class ValidateDiscountDTO
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>Email khach (kiem tra ma ca nhan + luot dung rieng); co the bo trong</summary>
        public string? Email { get; set; }

        /// <summary>Tam tinh gio hang — kiem tra DON TOI THIEU tren server (test case 7)</summary>
        public decimal Subtotal { get; set; }
    }
}
