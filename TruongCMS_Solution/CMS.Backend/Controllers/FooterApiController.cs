// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// API JSON cho FOOTER DONG — React doc de render chan trang

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Backend.Services;

namespace CMS.Backend.Controllers;

[Route("api/footer")]
[ApiController]
public class FooterApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cache;

    public FooterApiController(ApplicationDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// GET /api/footer — toan bo du lieu footer (mo ta, mang xa hoi, ban quyen,
    /// cac cot link). Cache 2 phut (footer it doi — muc 7 bao cao: caching).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var data = await _cache.GetOrCreateAsync("cache:footer", TimeSpan.FromMinutes(2), async () =>
        {
            var settings = await _context.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

            var columns = (await _context.FooterLinks
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.GroupHeading).ThenBy(l => l.SortOrder)
                    .ToListAsync())
                .GroupBy(l => l.GroupHeading)
                .Select(g => new
                {
                    heading = g.Key,
                    links = g.Select(l => new { l.Label, l.Url }).ToList()
                })
                .ToList();

            return new
            {
                description = settings.GetValueOrDefault("footer.description"),
                copyright   = settings.GetValueOrDefault("footer.copyright"),
                social = new
                {
                    facebook  = settings.GetValueOrDefault("social.facebook"),
                    messenger = settings.GetValueOrDefault("social.messenger"),
                    zalo      = settings.GetValueOrDefault("social.zalo"),
                },
                columns
            };
        });

        return Ok(data);
    }
}
