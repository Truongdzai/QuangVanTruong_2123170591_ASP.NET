using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Data.Entities
{
    // thuc the nguoi dung
    internal class User
    {
        public int Id { get; set; } // khoa chinh
        public string Username { get; set; } // ten dang nhap
        public string PasswordHash { get; set; } // ma hoa mat khau
        public string FullName { get; set; } // ho ten nguoi dung
        public string Role { get; set; } // vai tro nguoi dung (admin, editor, viewer)

    }
}
