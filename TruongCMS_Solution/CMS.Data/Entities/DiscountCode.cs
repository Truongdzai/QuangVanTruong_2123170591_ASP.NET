using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// Buổi 11 — Mã giảm giá / chương trình Sale (bảng DiscountCodes).
/// Admin tạo mã chung (SHOPCO…); hệ thống tự sinh mã chào mừng khi khách đăng ký.
/// </summary>
public class DiscountCode
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Mã giảm giá không được để trống!")]
    [StringLength(30)]
    public string Code { get; set; } = string.Empty;    // VD: SHOPCO, WELCOME-AB12

    [StringLength(200)]
    public string? Description { get; set; }            // Mô tả chương trình sale

    /// <summary>Phần trăm giảm trên tổng đơn (1–100)</summary>
    [Range(1, 100)]
    public int Percent { get; set; }

    /// <summary>Hết hạn (null = không giới hạn thời gian)</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Số lần dùng tối đa (0 = không giới hạn)</summary>
    public int MaxUses { get; set; }

    /// <summary>Đã dùng bao nhiêu lần (tăng khi đơn hàng áp mã thành công)</summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// ĐƠN TỐI THIỂU để được áp mã (0 = không yêu cầu) — test case 7:
    /// giỏ 450k nhập mã "đơn từ 500k" phải bị Backend từ chối.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Đơn tối thiểu không được âm!")]
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
    public decimal MinOrderAmount { get; set; }

    /// <summary>
    /// Số lần MỖI KHÁCH được dùng mã (0 = không giới hạn) — test case 8:
    /// mã 1 lần/khách thì lần đặt thứ hai phải báo "Mã đã được sử dụng".
    /// Đối chiếu qua bảng DiscountUsages.
    /// </summary>
    public int MaxUsesPerCustomer { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Mã cá nhân của 1 khách (null = mã công khai ai cũng dùng được)</summary>
    public int? CustomerId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
