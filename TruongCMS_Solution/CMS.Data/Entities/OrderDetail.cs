using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CMS.Data.Entities
{
    internal class OrderDetail
    {
        [Key]
        public int Id { get; set; } // khoa chinh
        public int OrderId { get; set; } // khoa ngoai
        public int ProductId { get; set; } // khoa ngoai
        public int Quantity { get; set; } // so luong san pham
        [Column(TypeName = "decimal(18,2)")] // định nghĩa kiểu dữ liệu decimal với độ chính xác 18 và 2 chữ số thập phân
        public decimal UnitPrice { get; set; } // gia san pham tai thoi diem dat hang
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; } // moi quan he voi don hang
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; } // moi quan he voi san pham
    }
}
