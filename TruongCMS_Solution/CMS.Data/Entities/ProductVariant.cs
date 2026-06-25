using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// BIẾN THỂ SẢN PHẨM (SKU) — mỗi tổ hợp Màu × Size là 1 dòng riêng,
/// có giá / tồn kho / ảnh riêng (Ví dụ: Áo thun Trắng - Size M).
/// Đáp ứng mục "1. Quản lý Sản phẩm & Kho hàng" của báo cáo nghiên cứu:
///  - Test case 1: đổi màu -> ảnh đổi theo (ImageUrl riêng từng màu)
///  - Test case 2: biến thể hết hàng -> chỉ biến thể đó bị khóa nút mua
///  - Test case 3: size lớn giá cao hơn (PriceOverride theo từng SKU)
/// </summary>
public class ProductVariant
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }

    /// <summary>Mã SKU duy nhất để quản kho (VD: TSHIRT-TRG-M)</summary>
    [StringLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>Mã màu HEX của biến thể (VD: #FFFFFF)</summary>
    [Required, StringLength(20)]
    public string Color { get; set; } = string.Empty;

    /// <summary>Tên màu hiển thị cho khách (VD: Trắng)</summary>
    [StringLength(50)]
    public string? ColorName { get; set; }

    /// <summary>Kích cỡ (VD: Small / Medium / Large / X-Large)</summary>
    [Required, StringLength(20)]
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// Giá riêng của biến thể; null = dùng giá gốc của Product.
    /// (Test case 3: size XXL có thể đặt giá cao hơn size S)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PriceOverride { get; set; }

    /// <summary>Tồn kho RIÊNG của biến thể này (0 = chỉ biến thể này hết hàng)</summary>
    public int StockQuantity { get; set; }

    /// <summary>Ảnh riêng theo màu; null = dùng ảnh đại diện của Product</summary>
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}
