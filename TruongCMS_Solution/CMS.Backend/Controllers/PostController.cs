// Họ tên : Quang Văn Trường | MSV: 2123170591
// Buổi 2 : Quản lý bài viết — danh sách + chi tiết từ bảng Posts
// Version: 1.2
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

/// <summary>
/// Controller bài viết (CMS).
/// - Index: danh sách card bài viết
/// - Details: xem 1 bài theo id trên URL (/Post/Details/5)
/// </summary>
public class PostController : Controller
{
    private readonly ApplicationDbContext _context;

    public PostController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET /Post/Index — danh sách bài viết, mới nhất lên đầu.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // OrderByDescending: sắp xếp theo CreatedDate giảm dần (LINQ → SQL ORDER BY)
        var posts = await _context.Posts
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(posts); // Views/Post/Index.cshtml
    }

    /// <summary>
    /// GET /Post/Details/{id} — chi tiết một bài viết.
    /// Tham số id lấy từ URL (ví dụ: /Post/Details/3 → id = 3).
    /// </summary>
    public async Task<IActionResult> Details(int id)
    {
        // FirstOrDefaultAsync: tìm bài có Id khớp; không có thì trả về null
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

        // HTTP 404 Not Found nếu id không tồn tại trong database
        if (post == null)
            return NotFound();

        return View(post); // Views/Post/Details.cshtml — @model Post (1 đối tượng)
    }
}
