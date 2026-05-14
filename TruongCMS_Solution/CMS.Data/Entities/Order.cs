using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CMS.Data.Entities
{
    internal class Order
    {
        [Key]
        public int Id { get; set; }// khoa chinh
        public DateTime OrderDate { get; set; } = DateTime.Now; // ngay dat hang
        public int CustomerId { get; set; } // khoa ngoai
        public int Status { get; set; } // trang thai don hang (0: dang xu ly, 1: da xac nhan, 2: da giao hang, 3: da huy)
        public string? Notes { get; set; } // ghi chu don hang
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; } // moi quan he voi khach hang
        public virtual ICollection<OrderDetail> OrderDetail { get; set; } // moi quan he voi chi tiet don hang


    }
}
