using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// LỊCH SỬ DÙNG MÃ GIẢM GIÁ (bảng DiscountUsages) — test case 8:
/// mã giới hạn 1 lần/khách: lưu lại AI đã dùng mã nào ở đơn nào,
/// lần sau nhập lại Backend tra bảng này và báo "Mã đã được sử dụng".
/// </summary>
public class DiscountUsage
{
    [Key]
    public int Id { get; set; }

    public int DiscountCodeId { get; set; }

    /// <summary>Email khách đã dùng (khách vãng lai cũng tính theo email)</summary>
    [Required, StringLength(150)]
    public string CustomerEmail { get; set; } = string.Empty;

    public int OrderId { get; set; }

    public DateTime UsedDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(DiscountCodeId))]
    public virtual DiscountCode? DiscountCode { get; set; }
}
