// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 11 (QUAN LY KHO — nhap hang tu cac hang)
// Ngay thuc hien: 12/06/2026
// Version: 2.0

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

/// <summary>
/// Quan ly NHAP KHO: tao phieu nhap hang tu cac hang (Coolmate, Nike...),
/// luu xong he thong tu CONG vao ton kho san pham; xoa phieu thi HOAN lai.
/// </summary>
[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // HELPER: do dropdown san pham (kem ton kho hien tai cho de chon)
    private void LoadProductList(int? selectedId = null)
    {
        var products = _context.Products
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, TenHienThi = p.Name + " (ton: " + p.StockQuantity + ")" })
            .ToList();
        ViewBag.ProductList = new SelectList(products, "Id", "TenHienThi", selectedId);
    }

    // GET /Inventory - lich su phieu nhap + thong ke nhanh
    public async Task<IActionResult> Index()
    {
        var receipts = await _context.StockReceipts
            .Include(r => r.Product)
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        // Thong ke nho tren dau trang
        ViewBag.TongPhieu = receipts.Count;
        ViewBag.TongSoLuong = receipts.Sum(r => r.Quantity);
        ViewBag.TongTienNhap = receipts.Sum(r => r.UnitCost * r.Quantity);

        return View(receipts);
    }

    // GET /Inventory/Create — lap phieu nhap moi
    [HttpGet]
    public IActionResult Create()
    {
        LoadProductList();
        return View(new StockReceipt { ReceiptDate = DateTime.Now, Quantity = 1 });
    }

    // POST /Inventory/Create — luu phieu + CONG ton kho san pham
    [HttpPost]
    public async Task<IActionResult> Create(StockReceipt model)
    {
        var product = await _context.Products.FindAsync(model.ProductId);
        if (product == null)
        {
            ModelState.AddModelError("ProductId", "Vui long chon san pham.");
            LoadProductList();
            return View(model);
        }

        if (model.Quantity <= 0)
        {
            ModelState.AddModelError("Quantity", "So luong nhap phai lon hon 0.");
            LoadProductList(model.ProductId);
            return View(model);
        }

        model.SupplierName = model.SupplierName.Trim();
        _context.StockReceipts.Add(model);

        // Nghiep vu chinh: nhap kho -> CONG vao ton kho san pham
        product.StockQuantity += model.Quantity;

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // GET /Inventory/Delete/{id} — xoa phieu nhap sai va HOAN lai ton kho (chi Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var receipt = await _context.StockReceipts
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt != null)
        {
            // Hoan tru ton kho (khong de am: phieu cu co the da ban bot hang)
            if (receipt.Product != null)
            {
                receipt.Product.StockQuantity = Math.Max(0, receipt.Product.StockQuantity - receipt.Quantity);
            }

            _context.StockReceipts.Remove(receipt);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
