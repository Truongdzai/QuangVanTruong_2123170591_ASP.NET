// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai

using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize(Roles = "Admin,Editor,Staff")]
public class VariantController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _upload;

    public VariantController(ApplicationDbContext context, IFileUploadService upload)
    {
        _context = context;
        _upload = upload;
    }

    // GET /Variant/Manage/{productId} — bang bien the cua 1 san pham
    public async Task<IActionResult> Manage(int id)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();
        return View(product);
    }

    // POST /Variant/Add — them 1 bien the (Mau × Size khong duoc trung)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, string color, string size,
        decimal? priceOverride, int stockQuantity, string? imageUrl, IFormFile? imageFile)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound();

        // CHAN GIA AM (logic price am)
        if (priceOverride is < 0)
        {
            TempData["Error"] = "Giá biến thể không được âm!";
            return RedirectToAction("Manage", new { id = productId });
        }

        // Admin go ten tieng Viet ("trắng") -> tu doi sang ma hex chuan cua shop
        string? hex = ColorHelper.ToHex(color);
        if (hex == null)
        {
            TempData["Error"] = $"Không nhận diện được màu \"{color}\" — nhập tên màu (trắng, đen...) hoặc mã hex (#FFFFFF).";
            return RedirectToAction("Manage", new { id = productId });
        }

        bool trung = await _context.ProductVariants
            .AnyAsync(v => v.ProductId == productId && v.Color == hex && v.Size == size.Trim());
        if (trung)
        {
            TempData["Error"] = $"Biến thể {ColorHelper.TenTiengViet(hex)} / {size} đã tồn tại.";
            return RedirectToAction("Manage", new { id = productId });
        }

        // Anh bien the: uu tien file UPLOAD, khong co thi dung URL dan tay
        string? anh = await _upload.UploadAsync(imageFile)
                      ?? (string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim());

        _context.ProductVariants.Add(new ProductVariant
        {
            ProductId     = productId,
            Sku           = $"SP{productId:D3}-{ColorHelper.TenTiengViet(hex).Replace(" ", "")}-{size.Trim()}",
            Color         = hex,
            ColorName     = ColorHelper.TenTiengViet(hex),
            Size          = size.Trim(),
            PriceOverride = priceOverride,
            StockQuantity = Math.Max(0, stockQuantity),
            ImageUrl      = anh,
            IsActive      = true
        });

        await DongBoTonKhoVaCsv(product);
        TempData["Success"] = "Đã thêm biến thể mới.";
        return RedirectToAction("Manage", new { id = productId });
    }

    // POST /Variant/Update — sua gia / ton kho / anh / bat tat cua 1 bien the
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, decimal? priceOverride,
        int stockQuantity, string? imageUrl, IFormFile? imageFile, bool isActive)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (variant?.Product == null) return NotFound();

        // CHAN GIA AM (logic price am)
        if (priceOverride is < 0)
        {
            TempData["Error"] = "Giá biến thể không được âm!";
            return RedirectToAction("Manage", new { id = variant.ProductId });
        }

        // File upload moi > URL dan tay > giu anh cu
        string? anhMoi = await _upload.UploadAsync(imageFile);
        if (anhMoi != null) variant.ImageUrl = anhMoi;
        else if (!string.IsNullOrWhiteSpace(imageUrl)) variant.ImageUrl = imageUrl.Trim();
        else if (imageUrl == "") variant.ImageUrl = null; // xoa anh khi de trong co chu dich

        variant.PriceOverride = priceOverride;
        variant.StockQuantity = Math.Max(0, stockQuantity);
        variant.IsActive      = isActive;

        await DongBoTonKhoVaCsv(variant.Product);
        TempData["Success"] = $"Đã cập nhật biến thể {variant.ColorName} / {variant.Size}.";
        return RedirectToAction("Manage", new { id = variant.ProductId });
    }

    // POST /Variant/Remove — xoa bien the (chan xoa neu da co don hang tham chieu)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (variant?.Product == null) return NotFound();

        int productId = variant.ProductId;

        bool daCoDon = await _context.OrderDetails.AnyAsync(d => d.ProductVariantId == id);
        if (daCoDon)
        {
            // Da nam trong don hang -> chi TAT (giu lich su), khong xoa cung
            variant.IsActive = false;
            await DongBoTonKhoVaCsv(variant.Product);
            TempData["Error"] = "Biến thể đã có trong đơn hàng — hệ thống chuyển sang TẮT thay vì xóa.";
        }
        else
        {
            _context.ProductVariants.Remove(variant);
            await DongBoTonKhoVaCsv(variant.Product);
            TempData["Success"] = "Đã xóa biến thể.";
        }

        return RedirectToAction("Manage", new { id = productId });
    }

    /// <summary>
    /// DONG BO sau moi thay doi bien the (muc 1 bao cao — "Dong bo ton kho"):
    ///  - Product.StockQuantity = tong ton cac bien the dang bat
    ///  - Cot CSV Colors/Sizes = tap mau/size dang bat (bo loc cua hang van chay dung)
    /// </summary>
    private async Task DongBoTonKhoVaCsv(Product product)
    {
        await _context.SaveChangesAsync(); // luu thay doi bien the truoc

        var variants = await _context.ProductVariants
            .Where(v => v.ProductId == product.Id && v.IsActive)
            .ToListAsync();

        if (variants.Count > 0)
        {
            product.StockQuantity = variants.Sum(v => v.StockQuantity);
            product.Colors = string.Join(",", variants.Select(v => v.Color).Distinct());
            product.Sizes  = string.Join(",", variants.Select(v => v.Size).Distinct());
        }

        await _context.SaveChangesAsync();
    }
}
