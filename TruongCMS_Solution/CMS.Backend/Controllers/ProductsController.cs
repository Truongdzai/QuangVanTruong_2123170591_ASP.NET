// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;

namespace CMS.Backend.Controllers
{
    // 1. Dinh nghia duong dan API. [controller] tu lay ten la "Products"
    // Khi chay, dia chi truy cap du lieu se la: https://localhost:xxxx/api/products
    [Route("api/[controller]")]

    // 2. Danh dau day la API Controller de he thong ho tro cac tinh nang
    //    tu dong kiem tra du lieu dau vao
    [ApiController]

    // 3. API Controller phai ke thua tu ControllerBase
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // 4. Ham khoi tao (Constructor): "Tiem" ngu canh du lieu SQL Server vao de su dung
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/products - Lay toan bo san pham (moi nhat len dau)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return Ok(products);
        }

        // GET api/products/categoryproduct/{categoryProductId}
        // Loc san pham theo danh muc san pham
        [HttpGet("categoryproduct/{categoryProductId}")]
        public async Task<IActionResult> GetByCategoryProduct(int categoryProductId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryProductId == categoryProductId)
                .ToListAsync();

            return Ok(products);
        }

        // GET api/products/{id} - Lay day du thong tin 1 san pham (bao gom Description)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm này trong hệ thống" });
            }

            return Ok(product);
        }
    }
}
