// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 8
// Ngay thuc hien: 07/06/2026
// Version: 1.8

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using System.Threading.Tasks;
using System.Linq;

namespace CMS.Backend.Controllers
{
    // 1. Cau hinh duong dan API: api/Categories
    //    (Khac voi /Category MVC tra ve View - controller nay tra ve JSON thuan cho ReactJS)
    [Route("api/[controller]")]

    // 2. Kich hoat tinh nang tu dong kiem tra loi du lieu (Validation)
    [ApiController]

    // 3. Ke thua ControllerBase de toi uu bo nho cho API thuan du lieu JSON
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 4. Ham khoi tao: Nap co so du lieu SQL Server vao Controller thong qua DI
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// BUOI 8 - Cua ngo du lieu cho bai tap mo rong ReactJS:
        /// API lay toan bo CHUYEN MUC TIN TUC (bang Categories - khac voi danh muc san pham).
        /// Duong dan goi du lieu: GET https://localhost:xxxx/api/Categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Buoc A: Quet bang Categories (chuyen muc bai viet) duoi SQL Server len
                var categories = await _context.Categories
                    .OrderBy(c => c.Name) // Sap xep theo ten cho de doc
                    .Select(c => new {
                        // Buoc B: Ky thuat got tia (Projection) - chi lay truong can cho FrontEnd
                        c.Id,
                        c.Name,
                        c.Description
                    })
                    .ToListAsync(); // Chuyen doi bat dong bo sang dang danh sach mang

                // Buoc C: Tra ve ma thanh cong HTTP 200 OK dinh kem chuoi JSON sach
                return Ok(categories);
            }
            catch (System.Exception ex)
            {
                // Bao ve he thong: Neu sap ket noi SQL thi tra ve loi 500 kem ly do
                return StatusCode(500, new {
                    message = "Lỗi kết nối cơ sở dữ liệu hệ thống",
                    detail = ex.Message
                });
            }
        }

        // GET api/Categories/{id} - Chi tiet 1 chuyen muc tin tuc
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy chuyên mục tin tức này" });
            }

            return Ok(category);
        }
    }
}
