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
public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /Customer - danh sach khach hang + TIM KIEM (ten/email/SDT) + PHAN TRANG
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        const int pageSize = 10;
        var query = _context.Customers.Include(c => c.Orders).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string kw = search.Trim();
            query = query.Where(c => c.FullName.Contains(kw) ||
                                     c.Email.Contains(kw) ||
                                     (c.Phone != null && c.Phone.Contains(kw)));
        }

        int total = await query.CountAsync();
        int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var data = await query.OrderByDescending(c => c.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Page = page; ViewBag.TotalPages = totalPages;
        ViewBag.Search = search; ViewBag.TotalItems = total;
        return View(data);
    }

    // GET /Customer/Details/{id} - chi tiet khach hang + lich su don hang
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Orders)
                .ThenInclude(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return NotFound();
        return View(customer);
    }

    // GET /Customer/Edit/{id} - sua thong tin lien he khach hang (Buoi 9 - CRUD day du)
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    // POST /Customer/Edit - cap nhat ho ten / SDT / dia chi (KHONG dung den mat khau da hash)
    [HttpPost]
    public async Task<IActionResult> Edit(int id, string fullName, string? phone, string? address)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ModelState.AddModelError("FullName", "Ho ten khong duoc de trong.");
            return View(customer);
        }

        customer.FullName = fullName.Trim();
        customer.Phone = phone;
        customer.Address = address;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id });
    }

    // GET /Customer/Delete/{id} - xoa khach hang (chi Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
