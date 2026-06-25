using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// Chi tiết đơn hàng — mỗi dòng = 1 sản phẩm trong đơn (bảng OrderDetails).
/// </summary>
public class OrderDetail
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }    // Thuộc đơn hàng nào
    public int ProductId { get; set; }  // Sản phẩm nào
    public int Quantity { get; set; }   // Số lượng mua

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } // Giá tại thời điểm đặt (có thể khác giá hiện tại)

    /// <summary>Biến thể SKU khách chọn (null = sản phẩm không có biến thể)</summary>
    public int? ProductVariantId { get; set; }

    /// <summary>Chốt nhãn biến thể tại thời điểm mua, VD "Trắng / Medium"</summary>
    [StringLength(100)]
    public string? VariantLabel { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    [ForeignKey(nameof(ProductVariantId))]
    public virtual ProductVariant? ProductVariant { get; set; }
}
