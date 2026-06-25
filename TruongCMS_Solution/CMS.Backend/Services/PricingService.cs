// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: gia hieu luc (flash sale check GIO SERVER)

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services;

/// <summary>Thong tin gia dang ap dung cho 1 san pham tai thoi diem hien tai.</summary>
public record GiaHieuLuc(
    decimal Gia,              // gia phai tra
    decimal GiaGoc,           // gia niem yet
    bool DangFlashSale,       // co dang trong khung gio sale khong
    DateTime? KetThucSale);   // de Frontend hien dong ho dem nguoc

/// <summary>
/// TINH GIA HIEU LUC cua san pham / bien the — muc 5 bao cao (Flash Sale).
/// Test case 9: moi lan tinh gia deu doi chieu KHUNG GIO SALE voi GIO SERVER;
/// het 12:00:00 thi 12:00:01 dat hang da ve gia goc, du Frontend con cache.
/// </summary>
public interface IPricingService
{
    /// <summary>Bang gia flash sale dang hieu luc: ProductId -> (gia sale, gio ket thuc)</summary>
    Task<Dictionary<int, (decimal SalePrice, DateTime EndTime)>> GetActiveFlashPricesAsync();

    /// <summary>Gia hieu luc cua 1 san pham (kem bien the neu co)</summary>
    Task<GiaHieuLuc> GetEffectivePriceAsync(Product product, ProductVariant? variant);
}

public class PricingService : IPricingService
{
    private readonly ApplicationDbContext _context;

    public PricingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<int, (decimal, DateTime)>> GetActiveFlashPricesAsync()
    {
        var now = DateTime.Now; // GIO SERVER — khong nhan thoi gian tu client

        var items = await _context.FlashSaleItems
            .Where(i => i.FlashSale != null
                        && i.FlashSale.IsActive
                        && i.FlashSale.StartTime <= now
                        && i.FlashSale.EndTime > now
                        // Con suat sale (0 = khong gioi han)
                        && (i.QuantityLimit == 0 || i.SoldCount < i.QuantityLimit))
            .Select(i => new { i.ProductId, i.SalePrice, i.FlashSale!.EndTime })
            .ToListAsync();

        // 1 san pham nam trong nhieu chuong trinh -> lay gia re nhat cho khach
        return items
            .GroupBy(i => i.ProductId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(i => i.SalePrice).Select(i => (i.SalePrice, i.EndTime)).First());
    }

    public async Task<GiaHieuLuc> GetEffectivePriceAsync(Product product, ProductVariant? variant)
    {
        // Gia niem yet: bien the co gia rieng (test case 3 — size lon gia cao hon)
        decimal giaGoc = variant?.PriceOverride ?? product.Price;

        var flash = await GetActiveFlashPricesAsync();
        if (flash.TryGetValue(product.Id, out var sale) && sale.Item1 < giaGoc)
        {
            return new GiaHieuLuc(sale.Item1, giaGoc, true, sale.Item2);
        }

        return new GiaHieuLuc(giaGoc, giaGoc, false, null);
    }
}
