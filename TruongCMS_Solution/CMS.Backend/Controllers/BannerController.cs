// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 11 (Quan ly BANNER DONG cho HeroSlider trang chu)
// Ngay thuc hien: 12/06/2026
// Version: 2.0

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

/// <summary>
/// Admin quan ly banner trang chu: them / sua / xoa / bat-tat / sap thu tu.
/// FrontEnd ReactJS doc banner qua API: GET /api/banners (BannersController).
/// </summary>
[Authorize]
public class BannerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public BannerController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // HELPER: luu file anh upload vao wwwroot/uploads, tra ve duong dan tuong doi
    private async Task<string?> UploadImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        string folder = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
        await file.CopyToAsync(stream);

        return "/uploads/" + fileName;
    }

    // GET /Banner - danh sach banner theo thu tu hien thi
    public async Task<IActionResult> Index()
    {
        var banners = await _context.Banners
            .OrderBy(b => b.SortOrder)
            .ThenBy(b => b.Id)
            .ToListAsync();
        return View(banners);
    }

    // GET /Banner/Create
    [HttpGet]
    public IActionResult Create() => View(new Banner { SortOrder = 1 });

    // POST /Banner/Create — anh nhan tu file upload HOAC o dan link (uu tien file)
    [HttpPost]
    public async Task<IActionResult> Create(Banner model, IFormFile? uploadImage)
    {
        string? uploaded = await UploadImageAsync(uploadImage);
        if (uploaded != null) model.ImageUrl = uploaded;

        if (string.IsNullOrWhiteSpace(model.ImageUrl))
        {
            ModelState.AddModelError("ImageUrl", "Vui long upload anh hoac dan link anh cho banner.");
            return View(model);
        }

        model.CreatedDate = DateTime.Now;
        _context.Banners.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // GET /Banner/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner == null) return NotFound();
        return View(banner);
    }

    // POST /Banner/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(Banner model, IFormFile? uploadImage)
    {
        var banner = await _context.Banners.FindAsync(model.Id);
        if (banner == null) return NotFound();

        string? uploaded = await UploadImageAsync(uploadImage);

        banner.Title      = model.Title;
        banner.Subtitle   = model.Subtitle;
        banner.LinkUrl    = model.LinkUrl;
        banner.ButtonText = model.ButtonText;
        banner.SortOrder  = model.SortOrder;
        banner.IsActive   = model.IsActive;
        banner.ImageUrl   = uploaded ?? (string.IsNullOrWhiteSpace(model.ImageUrl) ? banner.ImageUrl : model.ImageUrl);

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // GET /Banner/ToggleActive/{id} — bat/tat nhanh khong can vao form sua
    public async Task<IActionResult> ToggleActive(int id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner != null)
        {
            banner.IsActive = !banner.IsActive;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    // GET /Banner/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var banner = await _context.Banners.FindAsync(id);
        if (banner != null)
        {
            // Xoa file anh local neu la anh upload (khong xoa URL ngoai)
            if (banner.ImageUrl.StartsWith("/uploads/"))
            {
                var imgPath = Path.Combine(_env.WebRootPath, banner.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imgPath)) System.IO.File.Delete(imgPath);
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
