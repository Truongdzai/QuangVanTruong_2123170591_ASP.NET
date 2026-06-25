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

    // GET /Product - danh sach san pham + TIM KIEM + LOC DANH MUC + PHAN TRANG
    public async Task<IActionResult> Index(int page = 1, string? search = null, int? categoryId = null)
    {
        const int pageSize = 8; // 8 san pham moi trang

        // Truy van dong: loc theo tu khoa (ten/mo ta) + danh muc
        var query = _context.Products.Include(p => p.CategoryProduct).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string kw = search.Trim();
            query = query.Where(p => p.Name.Contains(kw) ||
                                     (p.Description != null && p.Description.Contains(kw)));
        }
        if (categoryId.HasValue && categoryId > 0)
            query = query.Where(p => p.CategoryProductId == categoryId.Value);

        int tongSp = await query.CountAsync();
        int tongTrang = Math.Max(1, (int)Math.Ceiling(tongSp / (double)pageSize));
        page = Math.Clamp(page, 1, tongTrang);

        var data = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Page        = page;
        ViewBag.TotalPages  = tongTrang;
        ViewBag.Search      = search;
        ViewBag.CategoryId  = categoryId;
        ViewBag.TotalItems  = tongSp;
        LoadCategoryProductList(categoryId);

        return View(data);
    }

    // HELPER: upload NHIEU file 1 luc, tra ve danh sach duong dan /uploads/...
    // Dung cho o chon anh duy nhat (chon 1 hay nhieu anh tuy y).
    private async Task<List<string>> UploadManyAsync(IEnumerable<IFormFile>? files)
    {
        var urls = new List<string>();
        if (files == null) return urls;

        foreach (var file in files)
        {
            var url = await UploadImageAsync(file);
            if (url != null) urls.Add(url);
        }
        return urls;
    }

    // GET /Product/Create
    [HttpGet]
    public IActionResult Create()
    {
        LoadCategoryProductList();
        return View(new Product());
    }

    // Tach cac URL anh admin DAN tay (moi dong/dau phay 1 URL)
    private static List<string> ParseImageUrls(string? raw) =>
        (raw ?? "").Split(new[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                   .Where(u => u.StartsWith("http") || u.StartsWith("/")).ToList();

    // POST /Product/Create — anh co the UPLOAD file HOAC DAN URL (hoac ca hai).
    // Anh DAU TIEN = anh dai dien (ImageUrl), cac anh sau = bo suu tap (GalleryUrls).
    [HttpPost]
    public async Task<IActionResult> Create(Product model, List<IFormFile>? uploadImages, string? imageUrlsText)
    {
        // CHAN GIA AM (logic price am) — khong tin client min="0"
        if (model.Price < 0)      ModelState.AddModelError(nameof(model.Price), "Giá bán không được âm!");
        if (model.StockQuantity < 0) ModelState.AddModelError(nameof(model.StockQuantity), "Tồn kho không được âm!");
        if (!ModelState.IsValid)
        {
            LoadCategoryProductList(model.CategoryProductId);
            return View(model);
        }

        // Gop anh upload + anh dan URL (upload truoc, URL sau)
        var urls = await UploadManyAsync(uploadImages);
        urls.AddRange(ParseImageUrls(imageUrlsText));

        model.ImageUrl    = urls.FirstOrDefault();
        model.GalleryUrls = urls.Count > 1 ? string.Join("\n", urls.Skip(1)) : null;

        // Admin go "trắng, đỏ" -> he thong tu doi sang "#FFFFFF,#F50606"
        model.Colors = ColorHelper.NormalizeColors(model.Colors);

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

    // POST /Product/Edit — chi 1 o chon anh nhu trang Create:
    // anh DAU TIEN thay anh dai dien, cac anh sau NOI THEM vao bo suu tap.
    // Tich "Xoa het anh cu" -> bo suu tap lam lai tu dau bang anh moi.
    [HttpPost]
    public async Task<IActionResult> Edit(Product model, List<IFormFile>? uploadImages,
        string? imageUrlsText, bool clearGallery = false)
    {
        // CHAN GIA AM (logic price am)
        if (model.Price < 0)         ModelState.AddModelError(nameof(model.Price), "Giá bán không được âm!");
        if (model.StockQuantity < 0) ModelState.AddModelError(nameof(model.StockQuantity), "Tồn kho không được âm!");
        if (!ModelState.IsValid)
        {
            LoadCategoryProductList(model.CategoryProductId);
            return View(model);
        }

        var old = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == model.Id);

        // Anh moi = upload + URL dan tay
        var urls = await UploadManyAsync(uploadImages);
        urls.AddRange(ParseImageUrls(imageUrlsText));

        if (urls.Count > 0)
        {
            model.ImageUrl = urls[0];
        }
        else if (old != null && string.IsNullOrEmpty(model.ImageUrl))
        {
            // Giu nguyen ImageUrl cu neu khong them anh moi
            model.ImageUrl = old.ImageUrl;
        }

        // Bo suu tap: giu anh cu (neu khong tich xoa) + noi them anh moi tu anh thu 2
        var gallery = new List<string>();
        if (!clearGallery && !string.IsNullOrWhiteSpace(old?.GalleryUrls))
            gallery.AddRange(old!.GalleryUrls!.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        gallery.AddRange(urls.Skip(1));
        model.GalleryUrls = gallery.Count > 0 ? string.Join("\n", gallery) : null;

        // Admin go "trắng, đỏ" -> he thong tu doi sang "#FFFFFF,#F50606"
        model.Colors = ColorHelper.NormalizeColors(model.Colors);

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
