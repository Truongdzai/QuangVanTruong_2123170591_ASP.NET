using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// Sản phẩm (bảng Products) — thuộc 1 CategoryProduct.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên sản phẩm không được để trống!")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, double.MaxValue)] // Giá >= 0
    [Column(TypeName = "decimal(18,2)")] // Kiểu tiền trong SQL Server
    public decimal Price { get; set; }

    public int StockQuantity { get; set; }  // Số lượng tồn kho
    public string? ImageUrl { get; set; }

    // Buổi 10 — LỌC THẬT theo màu & kích cỡ (khu Màu sắc / Kích cỡ ở trang Cửa hàng).
    // Lưu dạng CSV để đơn giản: Colors = "#000000,#063AF5" ; Sizes = "Small,Medium,Large"
    public string? Colors { get; set; }
    public string? Sizes { get; set; }

    // Buổi 11 — BỘ SƯU TẬP NHIỀU ẢNH: các URL cách nhau bằng xuống dòng hoặc dấu phẩy
    // (admin upload nhiều file hoặc dán nhiều link; FrontEnd hiện gallery ở trang chi tiết)
    public string? GalleryUrls { get; set; }

    public int CategoryProductId { get; set; } // Khóa ngoại

    /// <summary>Thương hiệu của sản phẩm (null = chưa gán) — mục 1 báo cáo</summary>
    public int? BrandId { get; set; }

    [ForeignKey(nameof(CategoryProductId))]
    public virtual CategoryProduct? CategoryProduct { get; set; }

    [ForeignKey(nameof(BrandId))]
    public virtual Brand? Brand { get; set; }

    /// <summary>
    /// Các biến thể SKU (Màu × Size). Khi sản phẩm CÓ biến thể thì tồn kho thật
    /// nằm ở từng biến thể; StockQuantity của Product = tổng các biến thể
    /// (được đồng bộ lại mỗi khi bán/nhập hàng).
    /// </summary>
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
