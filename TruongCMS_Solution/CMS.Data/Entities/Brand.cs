using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// THƯƠNG HIỆU / NHÃN HÀNG (bảng Brands) — mục "Danh mục &amp; Thương hiệu"
/// trong báo cáo nghiên cứu. Mỗi sản phẩm thuộc tối đa 1 thương hiệu.
/// </summary>
public class Brand
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên thương hiệu không được để trống!")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
