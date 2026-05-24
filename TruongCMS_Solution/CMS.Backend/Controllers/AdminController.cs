// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 4
// Ngay thuc hien: 23/03/2026
// Version: 1.4

using CMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /Admin - bang dieu khien, hien thong ke nhanh tinh trang website
    public async Task<IActionResult> Index()
    {
        // Dem tong so ban ghi moi bang -> hien len the thong ke
        ViewBag.TotalPosts      = await _context.Posts.CountAsync();
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.TotalUsers      = await _context.Users.CountAsync();

        // Lay 5 bai viet moi nhat kem ten danh muc de hien trong bang tom tat
        var recentPosts = await _context.Posts
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedDate)
            .Take(5)
            .ToListAsync();

        return View(recentPosts);
    }
}
