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

    // GET /CategoryProduct - danh sach danh muc SP + TIM KIEM + PHAN TRANG
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        const int pageSize = 8;
        var query = _context.CategoriesProducts.Include(c => c.Products).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string kw = search.Trim();
            query = query.Where(c => c.Name.Contains(kw) ||
                                     (c.Description != null && c.Description.Contains(kw)));
        }

        int total = await query.CountAsync();
        int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var data = await query.OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Page = page; ViewBag.TotalPages = totalPages;
        ViewBag.Search = search; ViewBag.TotalItems = total;
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
