// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 11 (Quan ly SALE / MA GIAM GIA)
// Ngay thuc hien: 12/06/2026
// Version: 2.0

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

/// <summary>
/// Admin quan ly chuong trinh sale: tao ma cong khai (SHOPCO...),
/// xem ca cac ma chao mung he thong tu sinh khi khach dang ky.
/// </summary>
[Authorize]
public class SaleController : Controller
{
    private readonly ApplicationDbContext _context;

    public SaleController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /Sale - danh sach ma giam gia (moi nhat len dau)
    public async Task<IActionResult> Index()
    {
        var codes = await _context.DiscountCodes
            .OrderByDescending(d => d.Id)
            .ToListAsync();
        return View(codes);
    }

    // GET /Sale/Create
    [HttpGet]
    public IActionResult Create() => View(new DiscountCode { Percent = 10, IsActive = true });

    // POST /Sale/Create
    [HttpPost]
    public async Task<IActionResult> Create(DiscountCode model)
    {
        model.Code = model.Code.Trim().ToUpper();

        // Ma phai duy nhat de khach nhap khong bi nham chuong trinh
        bool trungMa = await _context.DiscountCodes.AnyAsync(d => d.Code == model.Code);
        if (trungMa)
        {
            ModelState.AddModelError("Code", "Ma nay da ton tai, vui long chon ma khac.");
            return View(model);
        }

        model.UsedCount = 0;
        model.CreatedDate = DateTime.Now;
        _context.DiscountCodes.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // GET /Sale/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var code = await _context.DiscountCodes.FindAsync(id);
        if (code == null) return NotFound();
        return View(code);
    }

    // POST /Sale/Edit
    [HttpPost]
    public async Task<IActionResult> Edit(DiscountCode model)
    {
        var code = await _context.DiscountCodes.FindAsync(model.Id);
        if (code == null) return NotFound();

        string maMoi = model.Code.Trim().ToUpper();
        bool trungMa = await _context.DiscountCodes.AnyAsync(d => d.Code == maMoi && d.Id != model.Id);
        if (trungMa)
        {
            ModelState.AddModelError("Code", "Ma nay da ton tai, vui long chon ma khac.");
            return View(model);
        }

        code.Code        = maMoi;
        code.Description = model.Description;
        code.Percent     = model.Percent;
        code.ExpiryDate  = model.ExpiryDate;
        code.MaxUses     = model.MaxUses;
        code.IsActive    = model.IsActive;
        // Nang cap theo bao cao nghien cuu (test case 7-8)
        code.MinOrderAmount     = model.MinOrderAmount;
        code.MaxUsesPerCustomer = model.MaxUsesPerCustomer;

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // GET /Sale/ToggleActive/{id} — bat/tat nhanh
    public async Task<IActionResult> ToggleActive(int id)
    {
        var code = await _context.DiscountCodes.FindAsync(id);
        if (code != null)
        {
            code.IsActive = !code.IsActive;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    // GET /Sale/Delete/{id} (chi Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var code = await _context.DiscountCodes.FindAsync(id);
        if (code != null)
        {
            _context.DiscountCodes.Remove(code);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
