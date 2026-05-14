/* Họ tên :Quang Văn Trường
 * MSSV: 2123170591
 * Ngày tạo: 2026-05-14
 * version : 1.0*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Entities

{   //thuc the danh muc san phan
    internal class Category
    {
     public int Id { get; set; } // khoa chinh
        public string Name { get; set; } // ten danh muc san pham
        public string Description { get; set; } // mo ta danh muc san pham
        // moi quan he voi bai viet
        public virtual ICollection <Post> Potsts { get; set; }
    }
}
