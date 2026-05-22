// Họ tên: Quang Văn Trường | MSV: 2123170591
// Buổi 3: Thêm CRUD (Create, Delete, Edit) cho Category
// Version: 1.3

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /Category/Index — danh sách danh mục
    public async Task<IActionResult> Index()
    {
        var data = await _context.Categories.ToListAsync();
        return View(data);
    }

    // GET /Category/Create — hiển thị form thêm mới
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST /Category/Create — nhận dữ liệu từ form và lưu vào SQL
    [HttpPost]
    public IActionResult Create(Category model)
    {
        // Bước 1: Đăng ký vào bộ nhớ tạm của EF
        _context.Categories.Add(model);
        // Bước 2: Chốt — ghi xuống SQL Server (sinh ra câu INSERT INTO)
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET /Category/Edit/{id} — tìm và đổ dữ liệu cũ lên form
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST /Category/Edit — nhận dữ liệu đã sửa và cập nhật SQL
    [HttpPost]
    public IActionResult Edit(Category model)
    {
        _context.Categories.Update(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET /Category/Delete/{id} — xóa danh mục theo id rồi về Index
    public IActionResult Delete(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            // Bước 1: Đánh dấu "sẽ bị xóa" trong bộ nhớ tạm
            _context.Categories.Remove(category);
            // Bước 2: EF sinh ra câu DELETE FROM ... và gửi xuống SQL Server
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
