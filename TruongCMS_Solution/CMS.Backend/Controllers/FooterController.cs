// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Admin MVC quan ly FOOTER DONG: cot link + cau hinh site

using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize(Roles = "Admin,Editor")]
public class FooterController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;

    public FooterController(ApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET /Footer — danh sach link (gom nhom) + cau hinh site
    public async Task<IActionResult> Index()
    {
        ViewBag.Settings = await _context.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value ?? "");
        var links = await _context.FooterLinks
            .OrderBy(l => l.GroupHeading).ThenBy(l => l.SortOrder)
            .ToListAsync();
        return View(links);
    }

    // POST /Footer/AddLink
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLink(string groupHeading, string label, string url, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(groupHeading) || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(url))
        {
            TempData["Error"] = "Vui lòng nhập đủ Cột / Nhãn / Đường dẫn.";
            return RedirectToAction("Index");
        }

        _context.FooterLinks.Add(new FooterLink
        {
            GroupHeading = groupHeading.Trim(),
            Label = label.Trim(),
            Url = url.Trim(),
            SortOrder = sortOrder,
            IsActive = true
        });
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("cache:footer");
        TempData["Success"] = "Đã thêm liên kết footer.";
        return RedirectToAction("Index");
    }

    // POST /Footer/UpdateLink
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLink(int id, string groupHeading, string label, string url, int sortOrder, bool isActive)
    {
        var link = await _context.FooterLinks.FindAsync(id);
        if (link == null) return NotFound();

        link.GroupHeading = groupHeading.Trim();
        link.Label = label.Trim();
        link.Url = url.Trim();
        link.SortOrder = sortOrder;
        link.IsActive = isActive;
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("cache:footer");
        TempData["Success"] = "Đã lưu liên kết.";
        return RedirectToAction("Index");
    }

    // POST /Footer/DeleteLink
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLink(int id)
    {
        var link = await _context.FooterLinks.FindAsync(id);
        if (link != null)
        {
            _context.FooterLinks.Remove(link);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync("cache:footer");
        }
        return RedirectToAction("Index");
    }

    // POST /Footer/SaveSettings — luu mo ta / ban quyen / mang xa hoi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(string? description, string? copyright,
        string? facebook, string? messenger, string? zalo)
    {
        async Task Set(string key, string? value)
        {
            var s = await _context.SiteSettings.FindAsync(key);
            if (s == null) _context.SiteSettings.Add(new SiteSetting { Key = key, Value = value });
            else s.Value = value;
        }

        await Set("footer.description", description);
        await Set("footer.copyright", copyright);
        await Set("social.facebook", facebook);
        await Set("social.messenger", messenger);
        await Set("social.zalo", zalo);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync("cache:footer");

        TempData["Success"] = "Đã lưu cấu hình footer.";
        return RedirectToAction("Index");
    }
}
