// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 4
// Ngay thuc hien: 23/05/2026
// Version: 1.4

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

        // GET / - trang chu, hien 3 bai viet moi nhat bang LINQ
        public async Task<IActionResult> Index()
        {
            // .Include: lay kem ten danh muc de hien tren card
            // .OrderByDescending: sap xep moi nhat len dau
            // .Take(3): lay dung 3 bai dau tien
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

        // Trang loi, khong cache de luon hien loi moi nhat
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
