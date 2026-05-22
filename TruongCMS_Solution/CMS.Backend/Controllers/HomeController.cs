// Họ tên: Quang Văn Trường | MSV: 2123170591
// Buổi 3: Hiển thị 3 bài viết mới nhất lên trang chủ bằng LINQ
// Version: 1.3
using CMS.Backend.Models;
using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CMS.Backend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET / — trang chủ, hiển thị 3 bài viết mới nhất
        public async Task<IActionResult> Index()
        {
            // LINQ: Include Category (tránh null), sắp xếp mới nhất, lấy đúng 3 bài
            var latestPosts = await _context.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToListAsync();

            return View(latestPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
