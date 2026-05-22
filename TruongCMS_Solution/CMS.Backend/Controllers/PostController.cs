// Họ tên : Quang Văn Trường | MSV: 2123170591
// Buổi 3 : TRUY VẤN LINQ & THAO TÁC DỮ LIỆU CHUYÊN SÂU
// Version: 1.3 — thêm CRUD đầy đủ + upload ảnh local

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env; // Dùng để lấy đường dẫn wwwroot

    public PostController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ─── HELPER: lưu file ảnh vào wwwroot/images/posts/ ───────────────────────
    // Trả về đường dẫn tương đối (/images/posts/xxx.jpg) hoặc null nếu không có file
    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        // Đảm bảo thư mục tồn tại
        var folder = Path.Combine(_env.WebRootPath, "images", "posts");
        Directory.CreateDirectory(folder);

        // Tên file ngẫu nhiên để tránh trùng lặp
        var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Trả về URL tương đối dùng trong thẻ <img src="...">
        return $"/images/posts/{fileName}";
    }

    // ─── HELPER: đổ danh sách Category vào ViewBag để dùng trong dropdown ─────
    private void LoadCategoryDropdown(int? selectedId = null)
    {
        ViewBag.Categories = new SelectList(
            _context.Categories.OrderBy(c => c.Name).ToList(),
            "Id", "Name", selectedId);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // INDEX — danh sách bài viết, hỗ trợ lọc theo danh mục
    // GET /Post          → lấy tất cả
    // GET /Post/Index/5  → lọc theo CategoryId = 5
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(int? id)
    {
        var query = _context.Posts
            .Include(p => p.Category); // Eager Loading: tránh null khi gọi p.Category.Name

        if (id != null)
        {
            var filtered = await query
                .Where(p => p.CategoryId == id)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(filtered);
        }

        var all = await query
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(all);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DETAILS — chi tiết 1 bài viết
    // GET /Post/Details/5
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var post = await _context.Posts
            .Include(p => p.Category)      // Join bảng Category lấy tên danh mục
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();
        return View(post);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // CREATE — thêm bài viết mới
    // ──────────────────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create()
    {
        LoadCategoryDropdown();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Post model, IFormFile? imageFile)
    {
        // Xử lý upload ảnh; nếu không upload giữ null → View dùng ảnh mặc định
        model.ImageUrl = await SaveImageAsync(imageFile);
        model.CreatedDate = DateTime.Now;

        _context.Posts.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // EDIT — chỉnh sửa bài viết
    // ──────────────────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return NotFound();

        LoadCategoryDropdown(post.CategoryId);
        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Post model, IFormFile? imageFile)
    {
        // Chỉ thay ảnh khi người dùng có upload file mới
        if (imageFile != null && imageFile.Length > 0)
        {
            // Xóa ảnh cũ nếu là ảnh local (không xóa URL ngoài như picsum)
            if (!string.IsNullOrEmpty(model.ImageUrl)
                && model.ImageUrl.StartsWith("/images/"))
            {
                var oldPath = Path.Combine(_env.WebRootPath, model.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            model.ImageUrl = await SaveImageAsync(imageFile);
        }
        // Nếu không upload file mới → giữ nguyên ImageUrl cũ (đã bind từ hidden field)

        _context.Posts.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DELETE — xóa bài viết
    // GET /Post/Delete/5
    // ──────────────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post != null)
        {
            // Xóa file ảnh local nếu có
            if (!string.IsNullOrEmpty(post.ImageUrl)
                && post.ImageUrl.StartsWith("/images/"))
            {
                var imgPath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }
}
