// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize]
public class CategoryProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoryProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /CategoryProduct - danh sach danh muc san pham kem so luong SP
    public async Task<IActionResult> Index()
    {
        var data = await _context.CategoriesProducts
            .Include(c => c.Products)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(data);
    }

    // GET /CategoryProduct/Create
    [HttpGet]
    public IActionResult Create() => View();

    // POST /CategoryProduct/Create
    [HttpPost]
    public IActionResult Create(CategoryProduct model)
    {
        _context.CategoriesProducts.Add(model);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    // GET /CategoryProduct/Edit/{id}
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var item = _context.CategoriesProducts.Find(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST /CategoryProduct/Edit
    [HttpPost]
    public IActionResult Edit(CategoryProduct model)
    {
        _context.CategoriesProducts.Update(model);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    // GET /CategoryProduct/Delete/{id}
    public IActionResult Delete(int id)
    {
        var item = _context.CategoriesProducts.Find(id);
        if (item != null)
        {
            _context.CategoriesProducts.Remove(item);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}
