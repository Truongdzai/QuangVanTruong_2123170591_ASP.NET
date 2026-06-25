// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: may trang thai don hang + nghiep vu kem theo
// (test case 12: trang thai chi di MOT CHIEU, Backend cuong che — khong tin Frontend)

using CMS.Data;
using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Backend.Services;

/// <summary>Ket qua doi trang thai don.</summary>
public record KetQuaDoiTrangThai(bool ThanhCong, string? Loi, Order? Order);

/// <summary>
/// NGHIEP VU TRANG THAI DON HANG dung chung cho Admin MVC + Web API:
///  - Cuong che luong 1 chieu: Cho xac nhan -> Da gom hang -> Dang giao -> Thanh cong/Hoan tra
///  - Chuyen "Dang giao": tu dong day don sang hang van chuyen lay ma van don
///  - Huy / Hoan tra: HOAN KHO (tra lai ton kho bien the + san pham), hoan tien neu da thanh toan
///  - Thanh cong: cong DIEM TICH LUY cho khach (muc 4 bao cao — Loyalty)
/// </summary>
public interface IOrderWorkflowService
{
    Task<KetQuaDoiTrangThai> DoiTrangThaiAsync(int orderId, int trangThaiMoi, string? lyDo = null);
}

public class OrderWorkflowService : IOrderWorkflowService
{
    private readonly ApplicationDbContext _context;
    private readonly IShippingService _shipping;
    private readonly IConfiguration _config;
    private readonly ILogger<OrderWorkflowService> _logger;

    public OrderWorkflowService(
        ApplicationDbContext context,
        IShippingService shipping,
        IConfiguration config,
        ILogger<OrderWorkflowService> logger)
    {
        _context = context;
        _shipping = shipping;
        _config = config;
        _logger = logger;
    }

    public async Task<KetQuaDoiTrangThai> DoiTrangThaiAsync(int orderId, int trangThaiMoi, string? lyDo = null)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return new KetQuaDoiTrangThai(false, "Không tìm thấy đơn hàng.", null);

        // ── TEST CASE 12: chi cho phep buoc chuyen hop le, KHONG cho lui ─────
        if (!OrderStatusFlow.CanTransition(order.Status, trangThaiMoi))
        {
            return new KetQuaDoiTrangThai(false,
                $"Không thể chuyển từ \"{OrderStatusFlow.TenTrangThai(order.Status)}\" " +
                $"sang \"{OrderStatusFlow.TenTrangThai(trangThaiMoi)}\" — " +
                "quy trình đơn hàng chỉ đi một chiều.", order);
        }

        switch (trangThaiMoi)
        {
            // Chuyen sang DANG GIAO: day don sang hang van chuyen neu chua co ma van don
            case OrderStatusFlow.DangGiao when string.IsNullOrEmpty(order.TrackingCode):
                var kq = await _shipping.DayDonAsync(order);
                if (kq.ThanhCong)
                {
                    order.TrackingCode     = kq.TrackingCode;
                    order.ShippingProvider = kq.Provider;
                }
                break;

            // HUY don (tu Cho xac nhan / Da gom hang): hoan kho + hoan tien + tra luot ma
            case OrderStatusFlow.DaHuy:
                await HoanKhoAsync(order);
                await TraLuotMaGiamGiaAsync(order);
                HoanTienNeuDaThanhToan(order);
                order.CancelReason = lyDo ?? "Đã hủy";
                break;

            // HOAN TRA (tu Dang giao / Thanh cong): hoan kho + hoan tien, thu hoi diem
            case OrderStatusFlow.HoanTra:
                await HoanKhoAsync(order);
                HoanTienNeuDaThanhToan(order);
                ThuHoiDiemTichLuy(order);
                order.CancelReason = lyDo ?? "Khách hoàn trả hàng";
                break;

            // THANH CONG: cong diem tich luy; don COD coi nhu da thu tien
            case OrderStatusFlow.ThanhCong:
                CongDiemTichLuy(order);
                if (order.PaymentMethod == "COD" && order.PaymentStatus == 0)
                {
                    order.PaymentStatus = 1; // COD: giao thanh cong = da thu tien
                    order.PaidDate = DateTime.Now;
                }
                break;
        }

        order.Status = trangThaiMoi;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Don hang #{OrderId} chuyen sang trang thai {Status} ({Ten})",
            order.Id, trangThaiMoi, OrderStatusFlow.TenTrangThai(trangThaiMoi));

        return new KetQuaDoiTrangThai(true, null, order);
    }

    /// <summary>Tra lai ton kho cho tung mon trong don (ca bien the SKU lan ton kho tong)</summary>
    private async Task HoanKhoAsync(Order order)
    {
        foreach (var d in order.OrderDetails)
        {
            if (d.ProductVariantId.HasValue)
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE ProductVariants SET StockQuantity = StockQuantity + {d.Quantity} WHERE Id = {d.ProductVariantId.Value}");
            }
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE Products SET StockQuantity = StockQuantity + {d.Quantity} WHERE Id = {d.ProductId}");
        }
    }

    /// <summary>Don huy thi tra lai luot dung ma giam gia cho khach (test case 8 cong bang)</summary>
    private async Task TraLuotMaGiamGiaAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.DiscountCode)) return;

        var ma = await _context.DiscountCodes.FirstOrDefaultAsync(d => d.Code == order.DiscountCode);
        if (ma != null && ma.UsedCount > 0) ma.UsedCount--;

        var usage = await _context.DiscountUsages
            .FirstOrDefaultAsync(u => u.OrderId == order.Id);
        if (usage != null) _context.DiscountUsages.Remove(usage);
    }

    /// <summary>Don da thanh toan online -> danh dau HOAN TIEN (refund) tren don + giao dich</summary>
    private void HoanTienNeuDaThanhToan(Order order)
    {
        if (order.PaymentStatus != 1) return;

        order.PaymentStatus = 2; // 2 = da hoan tien
        var giaoDich = _context.PaymentTransactions
            .Where(t => t.OrderId == order.Id && t.Status == 1)
            .ToList();
        foreach (var t in giaoDich) t.Status = 3; // 3 = da hoan tien

        _logger.LogInformation("Don #{OrderId}: da tao lenh HOAN TIEN {Amount}",
            order.Id, order.TotalAmount);
    }

    /// <summary>Muc 4 bao cao — Loyalty: 1 diem cho moi 10 don vi tien cua don thanh cong</summary>
    private void CongDiemTichLuy(Order order)
    {
        if (order.Customer == null || order.LoyaltyPointsEarned > 0) return;

        int tyLe = int.TryParse(_config["Loyalty:AmountPerPoint"], out var t) ? t : 10;
        int diem = (int)(order.TotalAmount / Math.Max(1, tyLe));
        if (diem <= 0) return;

        order.LoyaltyPointsEarned = diem;
        order.Customer.LoyaltyPoints += diem;
    }

    /// <summary>Hoan tra sau khi da Thanh cong -> thu hoi diem da cong</summary>
    private void ThuHoiDiemTichLuy(Order order)
    {
        if (order.Customer == null || order.LoyaltyPointsEarned <= 0) return;

        order.Customer.LoyaltyPoints = Math.Max(0, order.Customer.LoyaltyPoints - order.LoyaltyPointsEarned);
        order.LoyaltyPointsEarned = 0;
    }
}
