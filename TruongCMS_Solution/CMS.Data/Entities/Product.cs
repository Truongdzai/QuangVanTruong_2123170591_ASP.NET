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

    public int CategoryProductId { get; set; } // Khóa ngoại

    [ForeignKey(nameof(CategoryProductId))]
    public virtual CategoryProduct? CategoryProduct { get; set; }
}
