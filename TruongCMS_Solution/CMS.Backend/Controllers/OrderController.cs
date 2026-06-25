// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: may trang thai don hang (test case 12)
// — Backend CUONG CHE luong 1 chieu, khong tin nut bam tren giao dien

using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderWorkflowService _workflow;

    public OrderController(ApplicationDbContext context, IOrderWorkflowService workflow)
    {
        _context = context;
        _workflow = workflow;
    }

    // GET /Order - danh sach don hang, loc duoc theo trang thai
    public async Task<IActionResult> Index(int? status)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        ViewBag.LocTrangThai = status;
        var data = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        return View(data);
    }

    // GET /Order/Details/{id} - chi tiet don + cac nut chuyen trang thai HOP LE
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // View chi hien cac buoc duoc phep di tiep (test case 12)
        ViewBag.BuocTiepTheo = OrderStatusFlow.AllowedTransitions
            .GetValueOrDefault(order.Status, Array.Empty<int>());

        return View(order);
    }

    /// <summary>
    /// POST /Order/UpdateStatus — di qua MAY TRANG THAI:
    /// buoc chuyen khong hop le (vd "Dang giao" quay ve "Cho xac nhan") bi TU CHOI
    /// du nguoi dung co tu che form gui len (khong tin giao dien — test case 12).
    /// Huy/hoan tra tu dong HOAN KHO; thanh cong tu dong cong DIEM TICH LUY.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, int status, string? reason)
    {
        var ketQua = await _workflow.DoiTrangThaiAsync(id, status, reason);

        if (!ketQua.ThanhCong)
        {
            TempData["Error"] = ketQua.Loi;
        }
        else
        {
            TempData["Success"] = $"Đã chuyển đơn #{id} sang \"{OrderStatusFlow.TenTrangThai(status)}\".";
        }

        return RedirectToAction("Details", new { id });
    }

    // GET /Order/Invoice/{id} - hoa don in duoc (Admin khong can email doi chieu)
    public async Task<IActionResult> Invoice(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order?.Customer == null) return NotFound();

        // Tai dung trang hoa don cua API (kem email chu don de qua kiem tra)
        return Redirect($"/api/orders/{id}/invoice?email={Uri.EscapeDataString(order.Customer.Email)}");
    }

    // GET /Order/Delete/{id} - xoa don hang (chi Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
