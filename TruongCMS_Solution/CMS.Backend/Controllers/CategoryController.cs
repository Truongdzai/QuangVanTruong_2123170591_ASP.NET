// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 4
// Ngay thuc hien: 23/05/2026
// Version: 1.4

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

    // GET /Category - hien danh sach danh muc lay tu SQL Server
    public async Task<IActionResult> Index()
    {
        var data = await _context.Categories.ToListAsync();
        return View(data);
    }

    // GET /Category/Create - hien form trong de nhap danh muc moi
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST /Category/Create - nhan du lieu tu form va ghi vao SQL
    [HttpPost]
    public IActionResult Create(Category model)
    {
        // Buoc 1: dang ky vao bo nho tam cua EF
        _context.Categories.Add(model);
        // Buoc 2: chot - ghi xuong SQL Server (sinh ra cau INSERT INTO)
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET /Category/Edit/{id} - tim danh muc, do du lieu cu len form
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null) return NotFound();

        return View(category);
    }

    // POST /Category/Edit - nhan du lieu da sua va cap nhat vao SQL
    [HttpPost]
    public IActionResult Edit(Category model)
    {
        _context.Categories.Update(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    // GET /Category/Delete/{id} - xoa danh muc theo id roi ve trang danh sach
    public IActionResult Delete(int id)
    {
        var category = _context.Categories.Find(id);
        if (category != null)
        {
            // Danh dau "se bi xoa" trong bo nho tam
            _context.Categories.Remove(category);
            // EF sinh ra cau DELETE FROM ... va gui xuong SQL Server
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
