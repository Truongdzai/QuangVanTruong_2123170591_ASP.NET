// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using CMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /Admin - bang dieu khien tong hop
    public async Task<IActionResult> Index()
    {
        // ---- Thong ke nhanh ----
        ViewBag.TotalPosts      = await _context.Posts.CountAsync();
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.TotalUsers      = await _context.Users.CountAsync();
        ViewBag.TotalProducts   = await _context.Products.CountAsync();
        ViewBag.TotalCustomers  = await _context.Customers.CountAsync();
        ViewBag.TotalOrders     = await _context.Orders.CountAsync();
        ViewBag.PendingOrders   = await _context.Orders.CountAsync(o => o.Status == 0);

        // ---- 5 bai viet moi nhat ----
        var recentPosts = await _context.Posts
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedDate)
            .Take(5)
            .ToListAsync();

        // ---- 5 don hang moi nhat ----
        ViewBag.RecentOrders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        return View(recentPosts);
    }
}
