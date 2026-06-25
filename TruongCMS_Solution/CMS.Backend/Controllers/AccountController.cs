// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 5
// Ngay thuc hien: 26/05/2026
// Version: 1.5

using CMS.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMS.Backend.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /Account/Login - hien form dang nhap
    [HttpGet]
    public IActionResult Login()
    {
        // Neu da dang nhap roi thi chuyen thang vao trang chu admin
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Admin");

        return View();
    }

    // POST /Account/Login - kiem tra thong tin va cap quyen
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Buoc 1: Tim tai khoan theo username, sau do doi chieu mat khau bang hash
        // (Tieu chi 33: khong luu/so sanh mat khau tho — dung SHA256 + Salt)
        var user = _context.Users.FirstOrDefault(u => u.Username == username);

        if (user != null && !PasswordHasher.Verify(password ?? "", user.PasswordHash))
        {
            user = null; // Sai mat khau -> xu ly nhu khong tim thay tai khoan
        }

        if (user != null)
        {
            // Du lieu cu con luu mat khau tho -> nhan dip dang nhap thanh cong de nang cap hash
            if (!PasswordHasher.IsHashed(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.Hash(password!);
                await _context.SaveChangesAsync();
            }

            // Buoc 2: Thiet lap danh tinh (Claims) - giong nhu noi dung chung minh nhan dan
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),   // Admin / Editor / Moderator / User
                new Claim("FullName", user.FullName)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Buoc 3: Dang nhap - luu Cookie vao trinh duyet
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Admin");
        }

        ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
        return View();
    }

    // GET /Account/Logout - xoa Cookie va ve trang Login
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // GET /Account/AccessDenied - trang bao "khong du quyen"
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
