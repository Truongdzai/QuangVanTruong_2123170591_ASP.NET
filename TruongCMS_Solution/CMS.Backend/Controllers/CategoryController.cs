using Microsoft.AspNetCore.Mvc;
using CMS.Data.Entities;// kết nối với thư mục chứa các thực thể dữ liệu, trong đó có Category
using System.Collections.Generic; // sử dụng thư viện để làm việc với danh sách
namespace CMS.Backend.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            // tạo danh sách dữ liệu mẫu cho các danh mục sản phẩm
            var list=new List<Category>
            {
                new Category { Id=1, Name="Điện thoại", Description="Các loại điện thoại thông minh" },
                new Category { Id=2, Name="Máy tính xách tay", Description="Các loại laptop và máy tính bảng" },
                new Category { Id=3, Name="Phụ kiện", Description="Các loại phụ kiện điện tử" }
            };
            return View(list); // trả về view và truyền danh sách dữ liệu mẫu vào view để hiển thị
        }
    }
}
