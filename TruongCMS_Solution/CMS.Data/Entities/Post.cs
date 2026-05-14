/* Họ tên :Quang Văn Trường
 * MSSV: 2123170591
 * version : 1.0*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Entities
{
    // thuc the bai viet
    internal class Post
    {
        public int Id { get; set; } // khoa chinh
        public string Title { get; set; } // tieu de bai viet
        public string Content { get; set; } // noi dung bai viet
        public string ImageUrl { get; set; } // duong dan hinh anh dai dien 
        public DateTime CreatedDate { get; set; }= DateTime.Now; // ngay tao bai viet
        // khóa ngoại liên kết với danh mục
        public int CategoryId { get; set; } // khoa ngoai
        public virtual Category Category { get; set; } // moi quan he voi danh muc san pham


    }
}
