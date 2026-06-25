using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// Đơn hàng (bảng Orders).
/// Status theo QUY TRÌNH XỬ LÝ của báo cáo nghiên cứu (mục 2 — Order &amp; Logistics):
///   0 = Chờ xác nhận -> 1 = Đã gom hàng -> 2 = Đang giao -> 3 = Thành công
///   nhánh kết thúc: 4 = Đã hủy (từ 0/1), 5 = Hoàn trả (từ 2/3).
/// Luồng đi MỘT CHIỀU — Backend chặn mọi cập nhật lùi trạng thái (test case 12),
/// xem máy trạng thái trong <see cref="OrderStatusFlow"/>.
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;
    public int CustomerId { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }

    // Buổi 11 — Mã giảm giá áp cho đơn (null = không dùng mã)
    public string? DiscountCode { get; set; }
    /// <summary>% giảm tại thời điểm đặt (chốt lại phòng khi mã bị sửa sau này)</summary>
    public int DiscountPercent { get; set; }

    // ── Thanh toán (mục 3 báo cáo — Payment Gateway) ─────────────────────────
    /// <summary>COD (mặc định) hoặc VNPAY</summary>
    [StringLength(20)]
    public string PaymentMethod { get; set; } = "COD";

    /// <summary>0 = Chưa thanh toán, 1 = Đã thanh toán, 2 = Đã hoàn tiền</summary>
    public int PaymentStatus { get; set; }

    /// <summary>Thời điểm IPN xác nhận tiền về (null = chưa thanh toán)</summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>Tổng tiền CHỐT của đơn (sau giảm giá + phí ship) — phục vụ báo cáo</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // ── Vận chuyển (mục 2 báo cáo — Logistics) ───────────────────────────────
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingFee { get; set; }

    /// <summary>Đơn vị vận chuyển đã đẩy đơn (GHTK / GHN / VTP)</summary>
    [StringLength(20)]
    public string? ShippingProvider { get; set; }

    /// <summary>Mã vận đơn do hãng vận chuyển cấp</summary>
    [StringLength(50)]
    public string? TrackingCode { get; set; }

    /// <summary>Lý do hủy / hoàn trả (khách hoặc admin nhập)</summary>
    [StringLength(300)]
    public string? CancelReason { get; set; }

    /// <summary>Điểm tích lũy khách nhận được khi đơn Thành công (mục 4 — Loyalty)</summary>
    public int LoyaltyPointsEarned { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

/// <summary>
/// MÁY TRẠNG THÁI đơn hàng — dùng chung cho cả Admin MVC lẫn Web API
/// để quy trình "Chờ xác nhận → Đã gom hàng → Đang giao → Thành công/Hoàn trả"
/// chỉ đi MỘT CHIỀU (test case 12: đơn Đang giao không thể quay về Chờ xác nhận).
/// </summary>
public static class OrderStatusFlow
{
    public const int ChoXacNhan = 0;
    public const int DaGomHang  = 1;
    public const int DangGiao   = 2;
    public const int ThanhCong  = 3;
    public const int DaHuy      = 4;
    public const int HoanTra    = 5;

    /// <summary>Các bước ĐƯỢC PHÉP chuyển tiếp từ mỗi trạng thái (rỗng = trạng thái cuối)</summary>
    public static readonly IReadOnlyDictionary<int, int[]> AllowedTransitions = new Dictionary<int, int[]>
    {
        [ChoXacNhan] = new[] { DaGomHang, DaHuy },
        [DaGomHang]  = new[] { DangGiao, DaHuy },
        [DangGiao]   = new[] { ThanhCong, HoanTra },
        [ThanhCong]  = new[] { HoanTra },          // khách trả hàng sau khi nhận
        [DaHuy]      = Array.Empty<int>(),
        [HoanTra]    = Array.Empty<int>(),
    };

    public static bool CanTransition(int from, int to)
        => AllowedTransitions.TryGetValue(from, out var nexts) && nexts.Contains(to);

    public static string TenTrangThai(int status) => status switch
    {
        ChoXacNhan => "Chờ xác nhận",
        DaGomHang  => "Đã gom hàng",
        DangGiao   => "Đang giao",
        ThanhCong  => "Thành công",
        DaHuy      => "Đã hủy",
        HoanTra    => "Hoàn trả",
        _          => $"Không rõ ({status})"
    };
}
