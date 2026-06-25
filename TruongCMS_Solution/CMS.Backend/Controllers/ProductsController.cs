// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU:
//  - Tra kem BIEN THE SKU (test case 1-3) + GIA FLASH SALE theo gio server (test case 9)
//  - Cache "hang moi ve"/"ban chay" bang IDistributedCache (muc 7 — Caching)
//  - Loc them theo thuong hieu (muc 1 — Danh muc & Thuong hieu)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricing;
        private readonly ICacheService _cache;

        public ProductsController(ApplicationDbContext context, IPricingService pricing, ICacheService cache)
        {
            _context = context;
            _pricing = pricing;
            _cache = cache;
        }

        /// <summary>
        /// Gan GIA FLASH SALE (neu dang trong khung gio — theo GIO SERVER) vao
        /// danh sach san pham tra ve cho Frontend.
        /// </summary>
        private async Task<List<object>> TrangTriGiaAsync(List<Product> products)
        {
            var flash = await _pricing.GetActiveFlashPricesAsync();

            return products.Select(p => (object)new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.ImageUrl,
                p.Colors,
                p.Sizes,
                p.GalleryUrls,
                p.CategoryProductId,
                p.BrandId,
                // Gia sale dang hieu luc (null = khong sale) + gio ket thuc de dem nguoc
                FlashPrice  = flash.TryGetValue(p.Id, out var f) && f.SalePrice < p.Price ? (decimal?)f.SalePrice : null,
                FlashEndsAt = flash.TryGetValue(p.Id, out var f2) && f2.SalePrice < p.Price ? (DateTime?)f2.EndTime : null
            }).ToList();
        }

        /// <summary>
        /// GET api/products — bo loc day du cho ReactJS:
        /// search / minPrice / maxPrice / categoryId / brandId / color / size / page.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] string? color,
            [FromQuery] string? size,
            [FromQuery] int? page,
            [FromQuery] int pageSize = 9)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string tuKhoa = search.Trim();
                query = query.Where(p => p.Name.Contains(tuKhoa) ||
                                         (p.Description != null && p.Description.Contains(tuKhoa)));
            }

            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
            if (brandId.HasValue && brandId > 0)
                query = query.Where(p => p.BrandId == brandId.Value);

            // LOC THAT theo mau & size: uu tien tra bang BIEN THE SKU (chinh xac,
            // chi tinh bien the con kinh doanh); fallback cot CSV cho san pham cu.
            if (!string.IsNullOrWhiteSpace(color))
            {
                string mau = color.Trim();
                string mauCsv = "," + mau + ",";
                query = query.Where(p =>
                    p.Variants.Any(v => v.Color == mau && v.IsActive) ||
                    (p.Colors != null && ("," + p.Colors + ",").Contains(mauCsv)));
            }

            if (!string.IsNullOrWhiteSpace(size))
            {
                string co = size.Trim();
                string coCsv = "," + co + ",";
                query = query.Where(p =>
                    p.Variants.Any(v => v.Size == co && v.IsActive) ||
                    (p.Sizes != null && ("," + p.Sizes + ",").Contains(coCsv)));
            }

            query = query.OrderByDescending(p => p.Id);

            if (!page.HasValue)
            {
                return Ok(await TrangTriGiaAsync(await query.ToListAsync()));
            }

            int coTrang = Math.Clamp(pageSize, 1, 50);
            int tongSanPham = await query.CountAsync();
            int tongTrang = (int)Math.Ceiling(tongSanPham / (double)coTrang);
            int trang = Math.Clamp(Math.Max(1, page.Value), 1, Math.Max(1, tongTrang));

            var items = await query
                .Skip((trang - 1) * coTrang)
                .Take(coTrang)
                .ToListAsync();

            return Ok(new
            {
                items = await TrangTriGiaAsync(items),
                totalItems = tongSanPham,
                page = trang,
                pageSize = coTrang,
                totalPages = tongTrang
            });
        }

        /// <summary>
        /// GET api/products/newest?count=3 — HANG MOI VE, cache 60 giay (muc 7 bao cao:
        /// trang chu goi lien tuc, cache giam han truy van SQL).
        /// </summary>
        [HttpGet("newest")]
        public async Task<IActionResult> GetNewest([FromQuery] int count = 3)
        {
            int soLuong = Math.Clamp(count, 1, 12);

            var products = await _cache.GetOrCreateAsync(
                $"{CacheService.KeyNewest}:{soLuong}",
                TimeSpan.FromSeconds(60),
                () => _context.Products.OrderByDescending(p => p.Id).Take(soLuong).ToListAsync());

            return Ok(await TrangTriGiaAsync(products ?? new List<Product>()));
        }

        /// <summary>GET api/products/bestsellers?count=3 — BAN CHAY (cache 60 giay)</summary>
        [HttpGet("bestsellers")]
        public async Task<IActionResult> GetBestSellers([FromQuery] int count = 3)
        {
            int soLuong = Math.Clamp(count, 1, 12);

            var ketQua = await _cache.GetOrCreateAsync(
                $"{CacheService.KeyBestSellers}:{soLuong}",
                TimeSpan.FromSeconds(60),
                async () =>
                {
                    var banChayIds = await _context.OrderDetails
                        .GroupBy(od => od.ProductId)
                        .Select(g => new { ProductId = g.Key, DaBan = g.Sum(od => od.Quantity) })
                        .OrderByDescending(x => x.DaBan)
                        .Take(soLuong)
                        .ToListAsync();

                    if (banChayIds.Count == 0)
                    {
                        return await _context.Products
                            .OrderByDescending(p => p.StockQuantity)
                            .Take(soLuong)
                            .ToListAsync();
                    }

                    var ids = banChayIds.Select(x => x.ProductId).ToList();
                    var products = await _context.Products
                        .Where(p => ids.Contains(p.Id))
                        .ToListAsync();

                    var list = banChayIds
                        .Select(x => products.FirstOrDefault(p => p.Id == x.ProductId))
                        .Where(p => p != null)
                        .Select(p => p!)
                        .ToList();

                    if (list.Count < soLuong)
                    {
                        var buThem = await _context.Products
                            .Where(p => !ids.Contains(p.Id))
                            .OrderByDescending(p => p.StockQuantity)
                            .Take(soLuong - list.Count)
                            .ToListAsync();
                        list.AddRange(buThem);
                    }
                    return list;
                });

            return Ok(await TrangTriGiaAsync(ketQua ?? new List<Product>()));
        }

        // GET api/products/categoryproduct/{categoryProductId}
        [HttpGet("categoryproduct/{categoryProductId}")]
        public async Task<IActionResult> GetByCategoryProduct(int categoryProductId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryProductId == categoryProductId)
                .ToListAsync();

            return Ok(await TrangTriGiaAsync(products));
        }

        /// <summary>
        /// GET api/products/{id} — chi tiet san pham KEM DANH SACH BIEN THE SKU:
        ///  - moi bien the co gia rieng (test case 3), ton kho rieng (test case 2),
        ///    anh rieng theo mau (test case 1)
        ///  - kem gia flash sale dang hieu luc (test case 9)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Variants.Where(v => v.IsActive))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm này trong hệ thống" });
            }

            var flash = await _pricing.GetActiveFlashPricesAsync();
            decimal? flashPrice = flash.TryGetValue(product.Id, out var f) && f.SalePrice < product.Price
                ? f.SalePrice : null;

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.ImageUrl,
                product.Colors,
                product.Sizes,
                product.GalleryUrls,
                product.CategoryProductId,
                product.BrandId,
                BrandName = product.Brand?.Name,
                FlashPrice  = flashPrice,
                FlashEndsAt = flashPrice != null ? (DateTime?)f.EndTime : null,
                Variants = product.Variants.Select(v => new
                {
                    v.Id,
                    v.Sku,
                    v.Color,
                    v.ColorName,
                    v.Size,
                    // Gia niem yet cua bien the (chua tinh flash sale)
                    Price = v.PriceOverride ?? product.Price,
                    v.StockQuantity,
                    v.ImageUrl
                })
            });
        }
    }

    /// <summary>GET /api/brands — danh sach thuong hieu (muc 1 bao cao), cache 5 phut</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cache;

        public BrandsController(ApplicationDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _cache.GetOrCreateAsync(
                CacheService.KeyBrands,
                TimeSpan.FromMinutes(5),
                async () => await _context.Brands
                    .Where(b => b.IsActive)
                    .OrderBy(b => b.Name)
                    .Select(b => new { b.Id, b.Name, b.LogoUrl, SoSanPham = b.Products.Count })
                    .ToListAsync());

            return Ok(brands);
        }
    }
}
