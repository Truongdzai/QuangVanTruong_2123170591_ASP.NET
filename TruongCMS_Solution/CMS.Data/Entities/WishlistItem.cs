using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// SẢN PHẨM YÊU THÍCH của khách hàng (bảng WishlistItems) —
/// mục "4. CRM: sản phẩm yêu thích (Wishlist)" trong báo cáo.
/// Mỗi khách chỉ có 1 dòng cho mỗi sản phẩm (unique CustomerId + ProductId).
/// </summary>
public class WishlistItem
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public int ProductId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}
