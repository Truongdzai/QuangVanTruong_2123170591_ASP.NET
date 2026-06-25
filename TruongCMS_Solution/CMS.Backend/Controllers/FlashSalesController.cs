// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: API Flash Sale (muc 5 — Marketing Engine)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Backend.Services;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlashSalesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheService _cache;

        public FlashSalesController(ApplicationDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        /// <summary>
        /// GET /api/flashsales/active — chuong trinh sale DANG CHAY theo GIO SERVER
        /// (test case 9). Frontend dung endTime de hien dong ho dem nguoc;
        /// het gio thi API tu tra ve rong -> gia tren web quay ve gia goc.
        /// Cache 30 giay (muc 7 — Caching) vi trang chu goi rat thuong xuyen.
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var data = await _cache.GetOrCreateAsync(
                CacheService.KeyFlashSale,
                TimeSpan.FromSeconds(30),
                async () =>
                {
                    var now = DateTime.Now; // GIO SERVER — khong tin dong ho client

                    return await _context.FlashSales
                        .Where(f => f.IsActive && f.StartTime <= now && f.EndTime > now)
                        .OrderBy(f => f.EndTime)
                        .Select(f => new
                        {
                            f.Id,
                            f.Name,
                            f.StartTime,
                            f.EndTime,
                            ServerTime = now, // de Frontend dem nguoc khop gio server
                            Items = f.Items.Select(i => new
                            {
                                i.ProductId,
                                ProductName = i.Product != null ? i.Product.Name : "",
                                ImageUrl = i.Product != null ? i.Product.ImageUrl : null,
                                OriginalPrice = i.Product != null ? i.Product.Price : 0,
                                i.SalePrice,
                                i.QuantityLimit,
                                i.SoldCount,
                                SoldOut = i.QuantityLimit > 0 && i.SoldCount >= i.QuantityLimit
                            })
                        })
                        .ToListAsync();
                });

            return Ok(data);
        }
    }
}
