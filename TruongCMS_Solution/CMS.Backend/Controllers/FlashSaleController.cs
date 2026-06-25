// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: quan tri FLASH SALE (muc 5 — Marketing Engine)

using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

/// <summary>
/// Trang quan tri FLASH SALE: tao chuong trinh theo KHUNG GIO, gan san pham
/// kem gia sale. Het gio he thong TU quay ve gia goc (PricingService check
/// gio server — test case 9), admin khong phai lam gi them.
/// </summary>
[Authorize(Roles = "Admin,Editor")]
public class FlashSaleController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;

    public FlashSaleController(ApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET /FlashSale — danh sach chuong trinh
    public async Task<IActionResult> Index()
    {
        var data = await _context.FlashSales
            .Include(f => f.Items)
            .OrderByDescending(f => f.StartTime)
            .ToListAsync();
        return View(data);
    }

    // GET /FlashSale/Create
    public IActionResult Create()
    {
        return View(new FlashSale
        {
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(3)
        });
    }

    // POST /FlashSale/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FlashSale model)
    {
        if (model.EndTime <= model.StartTime)
            ModelState.AddModelError(nameof(model.EndTime), "Giờ kết thúc phải sau giờ bắt đầu!");

        if (!ModelState.IsValid) return View(model);

        _context.FlashSales.Add(model);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync(CacheService.KeyFlashSale);

        TempData["Success"] = "Đã tạo chương trình — thêm sản phẩm vào khung giờ sale bên dưới.";
        return RedirectToAction("Edit", new { id = model.Id });
    }

    // GET /FlashSale/Edit/{id} — sua thong tin + quan ly san pham trong chuong trinh
    public async Task<IActionResult> Edit(int id)
    {
        var sale = await _context.FlashSales
            .Include(f => f.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (sale == null) return NotFound();

        // San pham chua nam trong chuong trinh de chon them
        var daCo = sale.Items.Select(i => i.ProductId).ToHashSet();
        ViewBag.SanPhamChuaCo = await _context.Products
            .Where(p => !daCo.Contains(p.Id))
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.Price })
            .ToListAsync();

        return View(sale);
    }

    // POST /FlashSale/Edit — cap nhat ten / khung gio / bat tat
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, DateTime startTime, DateTime endTime, bool isActive)
    {
        var sale = await _context.FlashSales.FindAsync(id);
        if (sale == null) return NotFound();

        if (endTime <= startTime)
        {
            TempData["Error"] = "Giờ kết thúc phải sau giờ bắt đầu!";
            return RedirectToAction("Edit", new { id });
        }

        sale.Name = name;
        sale.StartTime = startTime;
        sale.EndTime = endTime;
        sale.IsActive = isActive;
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync(CacheService.KeyFlashSale);

        TempData["Success"] = "Đã lưu chương trình flash sale.";
        return RedirectToAction("Edit", new { id });
    }

    // POST /FlashSale/AddItem — them san pham vao chuong trinh
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int flashSaleId, int productId, decimal salePrice, int quantityLimit)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            TempData["Error"] = "Sản phẩm không tồn tại.";
            return RedirectToAction("Edit", new { id = flashSaleId });
        }

        if (salePrice <= 0 || salePrice >= product.Price)
        {
            TempData["Error"] = $"Giá sale phải lớn hơn 0 và NHỎ HƠN giá gốc ({product.Price:N0}).";
            return RedirectToAction("Edit", new { id = flashSaleId });
        }

        bool daCo = await _context.FlashSaleItems
            .AnyAsync(i => i.FlashSaleId == flashSaleId && i.ProductId == productId);
        if (!daCo)
        {
            _context.FlashSaleItems.Add(new FlashSaleItem
            {
                FlashSaleId = flashSaleId,
                ProductId = productId,
                SalePrice = salePrice,
                QuantityLimit = quantityLimit
            });
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheService.KeyFlashSale);
        }

        return RedirectToAction("Edit", new { id = flashSaleId });
    }

    // POST /FlashSale/RemoveItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var item = await _context.FlashSaleItems.FindAsync(itemId);
        if (item == null) return NotFound();

        int flashSaleId = item.FlashSaleId;
        _context.FlashSaleItems.Remove(item);
        await _context.SaveChangesAsync();
        await _cache.RemoveAsync(CacheService.KeyFlashSale);

        return RedirectToAction("Edit", new { id = flashSaleId });
    }

    // GET /FlashSale/Delete/{id}
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var sale = await _context.FlashSales.FindAsync(id);
        if (sale != null)
        {
            _context.FlashSales.Remove(sale);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(CacheService.KeyFlashSale);
        }
        return RedirectToAction("Index");
    }
}
