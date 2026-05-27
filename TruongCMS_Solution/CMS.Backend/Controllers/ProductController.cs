// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // HELPER: upload anh san pham vao wwwroot/uploads
    private async Task<string?> UploadImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        string folder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(folder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return "/uploads/" + fileName;
    }

    // HELPER: do dropdown danh muc san pham
    private void LoadCategoryProductList(int? selectedId = null)
    {
        ViewBag.CategoryProductList = new SelectList(
            _context.CategoriesProducts.OrderBy(c => c.Name).ToList(),
            "Id", "Name", selectedId);
    }

    // GET /Product - danh sach san pham kem danh muc
    public async Task<IActionResult> Index()
    {
        var data = await _context.Products
            .Include(p => p.CategoryProduct)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
        return View(data);
    }

    // GET /Product/Create
    [HttpGet]
    public IActionResult Create()
    {
        LoadCategoryProductList();
        return View(new Product());
    }

    // POST /Product/Create
    [HttpPost]
    public async Task<IActionResult> Create(Product model, IFormFile? uploadImage)
    {
        model.ImageUrl = await UploadImageAsync(uploadImage);
        _context.Products.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // GET /Product/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        LoadCategoryProductList(product.CategoryProductId);
        return View(product);
    }

    // POST /Product/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(Product model, IFormFile? uploadImage)
    {
        if (uploadImage != null && uploadImage.Length > 0)
        {
            model.ImageUrl = await UploadImageAsync(uploadImage);
        }
        else
        {
            // Giu nguyen ImageUrl cu neu khong upload anh moi
            var old = await _context.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == model.Id);
            if (old != null && string.IsNullOrEmpty(model.ImageUrl))
                model.ImageUrl = old.ImageUrl;
        }

        _context.Products.Update(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // GET /Product/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            // Xoa file anh local neu la anh upload
            if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl.StartsWith("/uploads/"))
            {
                var imgPath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imgPath))
                    System.IO.File.Delete(imgPath);
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
