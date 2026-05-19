using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// Đơn hàng (bảng Orders).
/// Status: 0 = Chờ duyệt, 1 = Đang giao, 2 = Hoàn thành.
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;
    public int CustomerId { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
