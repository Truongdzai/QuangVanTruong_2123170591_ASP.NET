using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace CMS.Data.Entities
{
    // thuc the khach hang
    internal class Customer
    {
        [Key]
        public int Id { get; set; } // khoa chinh
        [Required] // ràng buộc không được để trống
        public string FullName { get; set; } // ho ten khach hang
        [Required]
        public string Email { get; set; } // email khach hang
        public string? Phone { get; set; } // so dien thoai khach hang
        public string? Address { get; set; } // dia chi khach hang
        [Required]
        public string Password { get; set; } //Lưu mật khẩu thô theo yêu ầu tối giản
        public virtual ICollection<Order> Orders { get; set; } // moi quan he voi don hang


    }
}
