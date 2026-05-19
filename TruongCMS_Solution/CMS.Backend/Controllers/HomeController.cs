// Trang chủ và trang lỗi mặc định của ASP.NET Core MVC

using CMS.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CMS.Backend.Controllers
{
    /// <summary>
    /// Controller mặc định — route: /Home/Index hoặc / (controller=Home).
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// GET / — trang chủ, có link tới Category, Post, User.
        /// </summary>
        public IActionResult Index()
        {
            return View(); // Views/Home/Index.cshtml (không truyền model)
        }

        /// <summary>
        /// GET /Home/Privacy — trang chính sách (mẫu có sẵn của template).
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Trang lỗi — không cache để luôn hiển thị lỗi mới nhất.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // RequestId dùng để debug / tra log khi có exception
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
