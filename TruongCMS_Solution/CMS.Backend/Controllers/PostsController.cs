// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;

namespace CMS.Backend.Controllers
{
    // Dinh nghia duong dan API: https://localhost:xxxx/api/posts
    [Route("api/[controller]")]
    // Kich hoat kiem tra du lieu dau vao tu dong va tra ve BadRequest neu sai
    [ApiController]
    // Ke thua ControllerBase (nhe hon Controller vi khong co tinh nang render View)
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/posts - Lay bai viet (got tia chi lay truong can thiet)
        // Buoi 9 (Tieu chi 14): ho tro ?page=&pageSize=&categoryId= de phan trang PostGrid.
        // Khong truyen page -> tra ve mang JSON thuan nhu cu (tuong thich nguoc).
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int pageSize = 6,
            [FromQuery] int? categoryId = null)
        {
            var query = _context.Posts.AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var projected = query
                .OrderByDescending(p => p.Id)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.ImageUrl,
                    p.CreatedDate,
                    p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                });

            if (!page.HasValue)
            {
                return Ok(await projected.ToListAsync());
            }

            // Phan trang Skip/Take duoi SQL Server
            int coTrang = Math.Clamp(pageSize, 1, 50);
            int tongBai = await projected.CountAsync();
            int tongTrangBai = (int)Math.Ceiling(tongBai / (double)coTrang);

            // Keo so trang ve khoang hop le (page qua lon -> tra trang cuoi)
            int trang = Math.Clamp(Math.Max(1, page.Value), 1, Math.Max(1, tongTrangBai));

            var items = await projected
                .Skip((trang - 1) * coTrang)
                .Take(coTrang)
                .ToListAsync();

            return Ok(new
            {
                items,
                totalItems = tongBai,
                page = trang,
                pageSize = coTrang,
                totalPages = (int)Math.Ceiling(tongBai / (double)coTrang)
            });
        }

        // GET api/posts/category/{categoryId} - Loc bai viet theo danh muc
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var posts = await _context.Posts
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.ImageUrl,
                    p.CreatedDate
                })
                .ToListAsync();

            return Ok(posts);
        }

        // GET api/posts/{id} - Lay chi tiet 1 bai viet day du (bao gom Content HTML)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết này trong hệ thống" });
            }

            return Ok(post);
        }
    }
}
