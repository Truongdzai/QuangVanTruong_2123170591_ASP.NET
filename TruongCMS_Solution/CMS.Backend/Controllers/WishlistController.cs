// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: Wishlist — san pham yeu thich (muc 4 CRM)

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using System.Security.Claims;

namespace CMS.Backend.Controllers
{
    /// <summary>
    /// DANH SACH YEU THICH cua khach hang — YEU CAU DANG NHAP (JWT Bearer).
    /// Goi khong kem token -> 401; token gia/het han -> 401 (Frontend tu refresh).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>Id khach hang lay tu claim "sub" trong JWT — khong tin Id Frontend gui len</summary>
        private int? CustomerId
        {
            get
            {
                string? raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue("sub");
                return int.TryParse(raw, out int id) ? id : null;
            }
        }

        /// <summary>GET /api/wishlist — danh sach yeu thich kem thong tin san pham</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (CustomerId is not int customerId) return Unauthorized();

            var items = await _context.WishlistItems
                .Where(w => w.CustomerId == customerId)
                .OrderByDescending(w => w.CreatedDate)
                .Select(w => new
                {
                    w.Id,
                    w.ProductId,
                    w.CreatedDate,
                    Product = w.Product == null ? null : new
                    {
                        w.Product.Id,
                        w.Product.Name,
                        w.Product.Price,
                        w.Product.ImageUrl,
                        w.Product.StockQuantity,
                        w.Product.CategoryProductId
                    }
                })
                .ToListAsync();

            return Ok(items);
        }

        /// <summary>GET /api/wishlist/ids — chi danh sach ProductId (to mau trai tim nhanh)</summary>
        [HttpGet("ids")]
        public async Task<IActionResult> GetIds()
        {
            if (CustomerId is not int customerId) return Unauthorized();

            var ids = await _context.WishlistItems
                .Where(w => w.CustomerId == customerId)
                .Select(w => w.ProductId)
                .ToListAsync();

            return Ok(ids);
        }

        /// <summary>POST /api/wishlist/{productId} — them vao yeu thich (bam lan 2 khong loi)</summary>
        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add(int productId)
        {
            if (CustomerId is not int customerId) return Unauthorized();

            bool sanPhamTonTai = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!sanPhamTonTai)
                return NotFound(new { message = "Sản phẩm không tồn tại." });

            bool daCo = await _context.WishlistItems
                .AnyAsync(w => w.CustomerId == customerId && w.ProductId == productId);

            if (!daCo)
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    CustomerId = customerId,
                    ProductId = productId
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã thêm vào danh sách yêu thích.", productId });
        }

        /// <summary>DELETE /api/wishlist/{productId} — bo yeu thich</summary>
        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId)
        {
            if (CustomerId is not int customerId) return Unauthorized();

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId);

            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Đã bỏ khỏi danh sách yêu thích.", productId });
        }
    }
}
