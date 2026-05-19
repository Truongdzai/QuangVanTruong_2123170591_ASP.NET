// Họ tên: Quang Văn Trường | MSV: 2123170591
// Buổi 2: Thay mock data (Buổi 1) bằng dữ liệu thật từ bảng Categories
//Version: 1.2
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

/// <summary>
/// Controller quản lý danh mục tin tức (bảng Categories).
/// Buổi 1: dùng List&lt;Category&gt; giả trong code.
/// Buổi 2: gọi _context.Categories từ SQL Server.
/// </summary>
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET /Category/Index — hiển thị bảng danh mục.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Lấy toàn bộ dòng trong bảng Categories
        var data = await _context.Categories.ToListAsync();

        // Gửi sang Views/Category/Index.cshtml (IEnumerable&lt;Category&gt;)
        return View(data);
    }
}
