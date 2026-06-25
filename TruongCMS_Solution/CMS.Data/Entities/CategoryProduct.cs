using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// Danh mục sản phẩm (E-commerce) — bảng CategoriesProducts.
/// </summary>
public class CategoryProduct
{
    [Key] // Đánh dấu khóa chính
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục sản phẩm không được để trống!")]
    [StringLength(100)] // Tối đa 100 ký tự trong SQL
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Buổi 9 — Ảnh đại diện ngành hàng cho khối CategoryMenu trang chủ (Tiêu chí 38)
    public string? ImageUrl { get; set; }

    /// <summary>
    /// CÂY DANH MỤC CHA - CON (mục 1 báo cáo): null = danh mục gốc,
    /// có giá trị = danh mục con của ParentId.
    /// </summary>
    public int? ParentId { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey(nameof(ParentId))]
    public virtual CategoryProduct? Parent { get; set; }

    public virtual ICollection<CategoryProduct> Children { get; set; } = new List<CategoryProduct>();

    // 1 danh mục SP có nhiều Product
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
