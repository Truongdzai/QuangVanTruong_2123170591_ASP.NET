// Họ tên: Quang Văn Trường | MSV: 2123170591
// Buổi 2: Controller quản lý thành viên — lấy dữ liệu từ SQL Server qua EF Core
//Version: 1.2
using CMS.Data;                          // ApplicationDbContext (lớp kết nối database)
using Microsoft.AspNetCore.Mvc;          // Controller, IActionResult, View()
using Microsoft.EntityFrameworkCore;     // ToListAsync() — truy vấn bất đồng bộ

namespace CMS.Backend.Controllers;

/// <summary>
/// Controller xử lý trang quản lý người dùng (bảng Users).
/// URL mặc định: /User hoặc /User/Index
/// </summary>
public class UserController : Controller
{
    // Biến readonly: chỉ gán 1 lần trong constructor, dùng xuyên suốt các Action
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Constructor Injection (DI): ASP.NET Core tự "tiêm" DbContext vào đây
    /// khi có request tới UserController — không cần new ApplicationDbContext() thủ công.
    /// </summary>
    public UserController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Action Index: hiển thị danh sách thành viên.
    /// - GET /User/Index
    /// - Trả về View kèm danh sách User (không trả PasswordHash ra giao diện).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // ToListAsync: chạy câu SQL SELECT * FROM Users, map sang List<User>
        var users = await _context.Users.ToListAsync();

        // View(users): tìm file Views/User/Index.cshtml và truyền danh sách vào @model
        return View(users);
    }
}
