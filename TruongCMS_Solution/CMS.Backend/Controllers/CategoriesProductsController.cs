// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using System.Threading.Tasks;
using System.Linq;

namespace CMS.Backend.Controllers
{
    // 1. Cau hinh duong dan API: api/CategoriesProducts
    [Route("api/[controller]")]

    // 2. Kich hoat tinh nang tu dong kiem tra loi du lieu (Validation)
    [ApiController]

    // 3. Ke thua ControllerBase de toi uu bo nho cho API thuan du lieu JSON
    public class CategoriesProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 4. Ham khoi tao: Nap co so du lieu SQL Server vao Controller thong qua DI
        public CategoriesProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// API lay toan bo danh muc san pham thoi trang (Giao thuc GET)
        /// Duong dan goi du lieu: GET https://localhost:xxxx/api/CategoriesProducts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Buoc A: Quet bang du lieu CategoriesProducts so nhieu duoi SQL Server len
                var categories = await _context.CategoriesProducts
                    .OrderBy(c => c.Name) // Sap xep theo ten
                    .Select(c => new {
                        // Buoc B: Ky thuat got tia (Projection) - chi lay cac truong can thiet ra FrontEnd
                        c.Id,
                        c.Name,
                        c.Description
                    })
                    .ToListAsync(); // Chuyen doi bat dong bo sang dang danh sach mang

                // Buoc C: Tra ve ma thanh cong HTTP 200 OK dinh kem chuoi chu JSON sach
                return Ok(categories);
            }
            catch (System.Exception ex)
            {
                // Bao ve he thong: Neu sap ket noi SQL thi tra ve loi 500 kem loi nhac ly do
                return StatusCode(500, new {
                    message = "Lỗi kết nối cơ sở dữ liệu hệ thống",
                    detail = ex.Message
                });
            }
        }

        // GET api/CategoriesProducts/{id} - Chi tiet 1 danh muc san pham
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.CategoriesProducts
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Không tìm thấy danh mục sản phẩm này" });
            }

            return Ok(category);
        }
    }
}
