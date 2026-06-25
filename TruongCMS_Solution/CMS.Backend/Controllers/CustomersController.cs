// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 9 (API Khach hang: Dang ky / Dang nhap / Quen mat khau - Tieu chi 33, 34, 46)
// Ngay thuc hien: 11/06/2026
// Version: 1.9

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CMS.Backend.Controllers
{
    // API JSON phuc vu ReactJS: https://localhost:xxxx/api/Customers/...
    // NANG CAP theo bao cao nghien cuu: dang nhap tra JWT + REFRESH TOKEN (test case 15)
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _email;
        private readonly ITokenService _tokens;

        public CustomersController(ApplicationDbContext context, IEmailService email, ITokenService tokens)
        {
            _context = context;
            _email = email;
            _tokens = tokens;
        }

        /// <summary>
        /// TIEU CHI 34 - Dang ky khach hang moi (CustomerRegister):
        /// 1) Kiem tra TRUNG LAP EMAIL trong SQL Server truoc khi luu
        /// 2) MA HOA mat khau (SHA256 + Salt) tu dong truoc khi luu ban ghi
        /// BUOI 11 (sua loi + mo rong):
        /// 3) Email tung dat hang khach vang lai (IsGuest) -> KICH HOAT tai khoan
        ///    thay vi bao trung email (truoc day khach checkout xong khong dang ky duoc)
        /// 4) Tang MA GIAM GIA CHAO MUNG 10%/30 ngay + gui qua email
        /// POST /api/Customers/register
        /// </summary>
        [HttpPost("register")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO input)
        {
            string email = input.Email.Trim().ToLower();
            var daCo = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email);

            Customer customer;

            if (daCo != null && !daCo.IsGuest)
            {
                // Tai khoan da ton tai -> chi duong ro rang cho khach (tranh "khong dang ky duoc" kho hieu)
                return Conflict(new
                {
                    message = "Email này đã có tài khoản (có thể được tạo khi bạn đặt hàng trước đó). " +
                              "Hãy đăng nhập, hoặc bấm 'Quên mật khẩu' để nhận mật khẩu mới qua email."
                });
            }

            if (daCo != null)
            {
                // SUA LOI DANG KY: email nay duoc he thong tu tao khi khach vang lai dat hang
                // -> cho phep "kich hoat" thanh tai khoan that (giu nguyen lich su don hang)
                customer = daCo;
                customer.FullName = input.FullName.Trim();
                customer.Phone    = input.Phone ?? customer.Phone;
                customer.Address  = input.Address ?? customer.Address;
                customer.Password = PasswordHasher.Hash(input.Password);
                customer.IsGuest  = false;
            }
            else
            {
                customer = new Customer
                {
                    FullName = input.FullName.Trim(),
                    Email    = email,
                    Phone    = input.Phone,
                    Address  = input.Address,
                    Password = PasswordHasher.Hash(input.Password),
                    IsGuest  = false
                };
                _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync();

            // BUOI 11: sinh MA GIAM GIA CHAO MUNG rieng cho khach moi (10%, han 30 ngay, dung 1 lan)
            string maChaoMung = "WELCOME-" + RandomNumberGenerator.GetInt32(1000, 9999);
            _context.DiscountCodes.Add(new DiscountCode
            {
                Code        = maChaoMung,
                Description = $"Mã chào mừng thành viên mới {customer.FullName}",
                Percent     = 10,
                ExpiryDate  = DateTime.Now.AddDays(30),
                MaxUses     = 1,
                IsActive    = true,
                CustomerId  = customer.Id
            });
            await _context.SaveChangesAsync();

            // Gui email chao mung kem ma giam gia (SMTP chua cau hinh -> ghi log, khong crash)
            await _email.SendAsync(
                customer.Email,
                "SHOP.CO - Chào mừng bạn! Tặng mã giảm giá 10%",
                $"<h2>Xin chào {customer.FullName},</h2>" +
                "<p>Cảm ơn bạn đã đăng ký thành viên SHOP.CO!</p>" +
                $"<p>Tặng bạn mã giảm giá <b style='font-size:22px'>{maChaoMung}</b> — giảm <b>10%</b> cho đơn hàng bất kỳ.</p>" +
                "<p>Mã có hiệu lực 30 ngày, dùng 1 lần. Nhập mã ở trang Giỏ hàng khi thanh toán nhé!</p>");

            // Dang ky xong cap luon cap token de khach dung ngay (khong bat login lai)
            var refreshTokenMoi = await _tokens.CreateRefreshTokenAsync(customer.Id);

            return StatusCode(201, new
            {
                message = "Đăng ký tài khoản thành công!",
                customer = new { customer.Id, customer.FullName, customer.Email, customer.Phone },
                // FrontEnd hien ma nay len man hinh sau khi dang ky
                welcomeCode = maChaoMung,
                welcomePercent = 10,
                accessToken = _tokens.CreateAccessToken(customer),
                refreshToken = refreshTokenMoi.Token
            });
        }

        /// <summary>
        /// Dang nhap khach hang (FrontEnd ReactJS) — doi chieu mat khau bang hash.
        /// POST /api/Customers/login
        /// </summary>
        [HttpPost("login")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginDTO input)
        {
            string email = input.Email.Trim().ToLower();
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email);

            if (customer == null || !PasswordHasher.Verify(input.Password, customer.Password))
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng!" });
            }

            // Du lieu cu con mat khau tho -> nang cap hash ngay khi dang nhap thanh cong
            if (!PasswordHasher.IsHashed(customer.Password))
            {
                customer.Password = PasswordHasher.Hash(input.Password);
                await _context.SaveChangesAsync();
            }

            // TEST CASE 15: cap ACCESS TOKEN (ngan han) + REFRESH TOKEN (dai han).
            // Access het han -> Frontend goi /refresh-token lay cap moi, khong bat login lai.
            var refreshToken = await _tokens.CreateRefreshTokenAsync(customer.Id);

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                customer = new { customer.Id, customer.FullName, customer.Email, customer.Phone, customer.Address },
                accessToken = _tokens.CreateAccessToken(customer),
                refreshToken = refreshToken.Token
            });
        }

        /// <summary>
        /// POST /api/Customers/refresh-token — doi refresh token lay CAP TOKEN MOI
        /// (xoay vong: token cu bi thu hoi ngay). Test case 15: khach treo may 1 ngay,
        /// access token het han -> Frontend goi ngam endpoint nay, khach khong biet gi.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO input)
        {
            var ketQua = await _tokens.RotateAsync(input.RefreshToken);

            if (ketQua == null)
            {
                // Token het han / da thu hoi / khong ton tai -> bat dang nhap lai
                return Unauthorized(new { message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            return Ok(new
            {
                accessToken = ketQua.Value.accessToken,
                refreshToken = ketQua.Value.refreshToken.Token
            });
        }

        /// <summary>POST /api/Customers/logout — thu hoi refresh token khi dang xuat</summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO input)
        {
            await _tokens.RevokeAsync(input.RefreshToken);
            return Ok(new { message = "Đã đăng xuất." });
        }

        /// <summary>
        /// GET /api/Customers/me — HO SO khach hang (can JWT): diem tich luy,
        /// hang thanh vien Bac/Vang/Kim cuong (muc 4 bao cao — Loyalty), thong ke don.
        /// </summary>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> Me()
        {
            string? rawId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(rawId, out int customerId)) return Unauthorized();

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return Unauthorized();

            int tongDon = await _context.Orders.CountAsync(o => o.CustomerId == customerId);
            int donThanhCong = await _context.Orders
                .CountAsync(o => o.CustomerId == customerId && o.Status == OrderStatusFlow.ThanhCong);
            decimal tongChiTieu = await _context.Orders
                .Where(o => o.CustomerId == customerId && o.Status == OrderStatusFlow.ThanhCong)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Cac ma giam gia ca nhan con dung duoc cua khach
            var maCaNhan = await _context.DiscountCodes
                .Where(d => d.CustomerId == customerId && d.IsActive
                            && (d.ExpiryDate == null || d.ExpiryDate > DateTime.Now)
                            && (d.MaxUses == 0 || d.UsedCount < d.MaxUses))
                .Select(d => new { d.Code, d.Percent, d.ExpiryDate })
                .ToListAsync();

            return Ok(new
            {
                customer.Id,
                customer.FullName,
                customer.Email,
                customer.Phone,
                customer.Address,
                loyaltyPoints = customer.LoyaltyPoints,
                membershipTier = customer.MembershipTier,
                totalOrders = tongDon,
                completedOrders = donThanhCong,
                totalSpent = tongChiTieu,
                personalCodes = maCaNhan
            });
        }

        /// <summary>
        /// POST /api/Customers/change-password — ĐỔI MẬT KHẨU khi đã đăng nhập:
        /// bắt buộc nhập ĐÚNG mật khẩu cũ + mật khẩu mới (đã được Frontend nhập 2 lần khớp).
        /// Cần JWT — chỉ chủ tài khoản đổi được mật khẩu của chính mình.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO input)
        {
            string? rawId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(rawId, out int customerId)) return Unauthorized();

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return Unauthorized();

            // Bắt buộc đúng mật khẩu cũ (chống người mượn máy đổi trộm)
            if (!PasswordHasher.Verify(input.OldPassword, customer.Password))
            {
                return BadRequest(new { message = "Mật khẩu cũ không đúng!" });
            }

            if (input.NewPassword == input.OldPassword)
            {
                return BadRequest(new { message = "Mật khẩu mới phải khác mật khẩu cũ." });
            }

            customer.Password = PasswordHasher.Hash(input.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công! Lần đăng nhập sau hãy dùng mật khẩu mới." });
        }

        /// <summary>PUT /api/Customers/me — cap nhat ho so (dia chi giao hang mac dinh...)</summary>
        [HttpPut("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDTO input)
        {
            string? rawId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(rawId, out int customerId)) return Unauthorized();

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(input.FullName)) customer.FullName = input.FullName.Trim();
            if (input.Phone != null)   customer.Phone = input.Phone;
            if (input.Address != null) customer.Address = input.Address;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã cập nhật hồ sơ.",
                customer = new { customer.Id, customer.FullName, customer.Email, customer.Phone, customer.Address }
            });
        }

        /// <summary>
        /// TIEU CHI 46 + BUOI 11 (luong moi theo yeu cau) - QUEN MAT KHAU:
        /// nguoi dung nhap dung email -> he thong RESET ngay sang MAT KHAU MOI
        /// ngau nhien va GUI MAT KHAU MOI VE EMAIL cua khach.
        /// (SMTP chua cau hinh -> tra mat khau demo trong response de cham bai offline)
        /// POST /api/Customers/forgot-password
        /// </summary>
        [HttpPost("forgot-password")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO input)
        {
            string email = input.Email.Trim().ToLower();
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email);

            if (customer == null)
            {
                return NotFound(new { message = "Email này chưa được đăng ký tài khoản." });
            }

            // Sinh MAT KHAU MOI de doc: 2 tu + 4 so (vd "ShopCo-4821")
            string matKhauMoi = "ShopCo-" + RandomNumberGenerator.GetInt32(100000, 999999);

            // Bam hash roi luu ngay — mat khau cu het hieu luc tu giay phut nay
            customer.Password = PasswordHasher.Hash(matKhauMoi);
            customer.ResetToken = null;
            customer.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            bool daGui = await _email.SendAsync(
                customer.Email,
                "SHOP.CO - Mật khẩu mới của bạn",
                $"<h3>Xin chào {customer.FullName},</h3>" +
                "<p>Bạn (hoặc ai đó) vừa yêu cầu đặt lại mật khẩu tại SHOP.CO.</p>" +
                $"<p>Mật khẩu mới của bạn là: <b style='font-size:22px'>{matKhauMoi}</b></p>" +
                "<p>Hãy đăng nhập bằng mật khẩu này và đổi lại mật khẩu riêng của bạn ngay sau đó.</p>");

            return Ok(new
            {
                message = daGui
                    ? "Mật khẩu mới đã được gửi tới email của bạn. Hãy kiểm tra hộp thư (kể cả mục Spam)."
                    : "SMTP chưa cấu hình — dùng mật khẩu demo bên dưới để đăng nhập.",
                // Chi tra ve khi khong gui duoc email that (demo offline)
                demoPassword = daGui ? null : matKhauMoi
            });
        }

        /// <summary>
        /// TIEU CHI 46 - Quen mat khau (buoc 2): kiem tra ma xac nhan con han,
        /// sau do bam mat khau moi va xoa ma de khong dung lai duoc.
        /// POST /api/Customers/reset-password
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO input)
        {
            string email = input.Email.Trim().ToLower();
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == email);

            if (customer == null || customer.ResetToken != input.Token)
            {
                return BadRequest(new { message = "Mã xác nhận không đúng!" });
            }

            if (customer.ResetTokenExpiry == null || customer.ResetTokenExpiry < DateTime.Now)
            {
                return BadRequest(new { message = "Mã xác nhận đã hết hạn, vui lòng yêu cầu mã mới." });
            }

            customer.Password = PasswordHasher.Hash(input.NewPassword);
            customer.ResetToken = null;
            customer.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đặt lại mật khẩu thành công! Hãy đăng nhập bằng mật khẩu mới." });
        }
    }

    // CAC LOP DTO HUNG DU LIEU JSON TU REACTJS GUI LEN

    public class RegisterDTO
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Address { get; set; }

        [Required, MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class RefreshTokenDTO
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class UpdateProfileDTO
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        public string OldPassword { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordDTO
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
