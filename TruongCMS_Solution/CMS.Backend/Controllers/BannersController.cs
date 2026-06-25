// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 11 (API banner dong cho HeroSlider ReactJS)
// Ngay thuc hien: 12/06/2026
// Version: 2.0

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;

namespace CMS.Backend.Controllers
{
    /// <summary>
    /// API JSON: GET /api/banners — tra ve banner DANG BAT theo thu tu,
    /// FrontEnd dung lam slide dau tien cua HeroSlider trang chu.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BannersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BannersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/banners — chi lay banner IsActive, sap theo SortOrder
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Subtitle,
                    b.ImageUrl,
                    b.LinkUrl,
                    b.ButtonText
                })
                .ToListAsync();

            return Ok(banners);
        }
    }
}
