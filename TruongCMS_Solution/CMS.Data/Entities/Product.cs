using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CMS.Data.Entities
{
    // thuc the san pham
    internal class Product
    {
        public int Id { get; set; } // khoa chinh
        [Required(ErrorMessage = "Tên sản phẩm không được để trống!")] // ràng buộc không được để trống
        public string Name { get; set; } // ten san pham
        public string? Description { get; set; } // mo ta san pham
        [Range(0, double.MaxValue)] // ràng buộc giá phải lớn hơn hoặc bằng 0
        [Column(TypeName = "decimal(18,2)")] // định nghĩa kiểu dữ liệu decimal với độ chính xác 18 và 2 chữ số thập phân
        public decimal Price { get; set; } // gia san pham
        public int StockQuantity { get; set; } // so luong ton kho
        // khóa ngoai đến danh mục sản phẩm
        [ForeignKey("CategoryProduc")]
        public virtual CategoryProduct? CategoryProduct { get; set; } // moi quan he voi danh muc san pham

    }
}