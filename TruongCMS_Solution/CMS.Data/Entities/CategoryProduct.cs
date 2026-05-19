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

    // 1 danh mục SP có nhiều Product
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
