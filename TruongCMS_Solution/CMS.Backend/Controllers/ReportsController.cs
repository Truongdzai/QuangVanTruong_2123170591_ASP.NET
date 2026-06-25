// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: BAO CAO & THONG KE (muc 6 — Business Intelligence)
//  - Doanh thu theo ngay/thang/nam + ty le tang truong
//  - Top san pham ban chay / ton kho lau
//  - Ty le khach quay lai mua lan 2 (retention rate)

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Controllers;

[Authorize(Roles = "Admin,Accountant")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var homNay = DateTime.Today;

        // Chi tinh doanh thu don THANH CONG (tien that ve tui)
        var donThanhCong = _context.Orders.Where(o => o.Status == OrderStatusFlow.ThanhCong);

        // ── 1. DOANH THU 30 NGAY GAN NHAT (bieu do duong) ────────────────────
        var tu30Ngay = homNay.AddDays(-29);
        var theoNgayRaw = await donThanhCong
            .Where(o => o.OrderDate >= tu30Ngay)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new { Ngay = g.Key, DoanhThu = g.Sum(o => o.TotalAmount), SoDon = g.Count() })
            .ToListAsync();

        var doanhThu30Ngay = Enumerable.Range(0, 30)
            .Select(i => tu30Ngay.AddDays(i))
            .Select(ngay => new
            {
                Nhan = ngay.ToString("dd/MM"),
                DoanhThu = theoNgayRaw.FirstOrDefault(x => x.Ngay == ngay)?.DoanhThu ?? 0,
                SoDon = theoNgayRaw.FirstOrDefault(x => x.Ngay == ngay)?.SoDon ?? 0
            })
            .ToList();

        // ── 2. DOANH THU 12 THANG (bieu do cot) + TANG TRUONG ────────────────
        var tu12Thang = new DateTime(homNay.Year, homNay.Month, 1).AddMonths(-11);
        var theoThangRaw = await donThanhCong
            .Where(o => o.OrderDate >= tu12Thang)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, DoanhThu = g.Sum(o => o.TotalAmount) })
            .ToListAsync();

        var doanhThu12Thang = Enumerable.Range(0, 12)
            .Select(i => tu12Thang.AddMonths(i))
            .Select(thang => new
            {
                Nhan = thang.ToString("MM/yyyy"),
                DoanhThu = theoThangRaw
                    .FirstOrDefault(x => x.Year == thang.Year && x.Month == thang.Month)?.DoanhThu ?? 0
            })
            .ToList();

        // Tang truong = (thang nay - thang truoc) / thang truoc
        decimal thangNay = doanhThu12Thang[^1].DoanhThu;
        decimal thangTruoc = doanhThu12Thang[^2].DoanhThu;
        decimal? tangTruong = thangTruoc > 0
            ? Math.Round((thangNay - thangTruoc) / thangTruoc * 100, 1)
            : null;

        // ── 3. TOP 5 SAN PHAM BAN CHAY (theo so luong da ban) ────────────────
        var topBanChay = await _context.OrderDetails
            .Where(d => d.Order != null && d.Order.Status == OrderStatusFlow.ThanhCong)
            .GroupBy(d => new { d.ProductId, Ten = d.Product!.Name })
            .Select(g => new
            {
                g.Key.Ten,
                DaBan = g.Sum(d => d.Quantity),
                DoanhThu = g.Sum(d => d.Quantity * d.UnitPrice)
            })
            .OrderByDescending(x => x.DaBan)
            .Take(5)
            .ToListAsync();

        // ── 4. SAN PHAM TON KHO LAU: chua ban duoc don nao / ton nhieu nhat ──
        var idDaBan = await _context.OrderDetails.Select(d => d.ProductId).Distinct().ToListAsync();
        var tonKhoLau = await _context.Products
            .Where(p => !idDaBan.Contains(p.Id))
            .OrderByDescending(p => p.StockQuantity)
            .Take(5)
            .Select(p => new { p.Name, p.StockQuantity, p.Price })
            .ToListAsync();

        // ── 5. RETENTION RATE: % khach (da mua) quay lai mua lan 2 tro len ───
        var donTheoKhach = await _context.Orders
            .Where(o => o.Status != OrderStatusFlow.DaHuy) // don huy khong tinh
            .GroupBy(o => o.CustomerId)
            .Select(g => g.Count())
            .ToListAsync();

        int khachDaMua = donTheoKhach.Count;
        int khachQuayLai = donTheoKhach.Count(soDon => soDon >= 2);
        decimal retention = khachDaMua > 0
            ? Math.Round((decimal)khachQuayLai / khachDaMua * 100, 1) : 0;

        // ── 6. CON SO TONG QUAN ──────────────────────────────────────────────
        ViewBag.TongDoanhThu   = await donThanhCong.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        ViewBag.DoanhThuThang  = thangNay;
        ViewBag.TangTruong     = tangTruong;
        ViewBag.TongDon        = await _context.Orders.CountAsync();
        ViewBag.DonChoXuLy     = await _context.Orders.CountAsync(o => o.Status == OrderStatusFlow.ChoXacNhan);
        ViewBag.KhachDaMua     = khachDaMua;
        ViewBag.KhachQuayLai   = khachQuayLai;
        ViewBag.Retention      = retention;
        ViewBag.SapHetHang     = await _context.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= 5);
        ViewBag.HetHang        = await _context.Products.CountAsync(p => p.StockQuantity == 0);

        ViewBag.DoanhThu30Ngay  = doanhThu30Ngay;
        ViewBag.DoanhThu12Thang = doanhThu12Thang;
        ViewBag.TopBanChay      = topBanChay;
        ViewBag.TonKhoLau       = tonKhoLau;

        return View();
    }
}
