using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace CMS.Data.Entities;

    // thuc the lien ket giua danh muc sp và  san pham
    internal class CategoryProduct
{
    [Key]
    public int Id { get; set; } // khoa chinh
    [Required(ErrorMessage = "Tên danh mục sản phẩm không được để trống!")] // ràng buộc không được để trống
    [StringLength(100, ErrorMessage = "Tên danh mục sản phẩm không được vượt quá 100 ký tự!")]// ràng buộc độ dài tối đa 100 ký tự
    public string Name { get; set; }  // ten danh muc san pham
    public string? Description { get; set; } // mo ta danh muc san pham
    // quan hệ: 1-n với sản phẩm
    public virtual ICollection<Product> Products { get; set; } // moi quan he voi san pham
}
