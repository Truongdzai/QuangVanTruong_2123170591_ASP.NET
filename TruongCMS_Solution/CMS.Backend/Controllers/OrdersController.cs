// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using Microsoft.AspNetCore.Mvc;
using CMS.Data;
using CMS.Data.Entities;
using System;
using System.Threading.Tasks;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// API: Tiep nhan don dat hang tu gio hang FrontEnd gui len
        /// Duong dan: POST https://localhost:xxxx/api/Orders
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderInputDTO input)
        {
            // 1. Kiem tra kich ban loi bao ve: Neu du lieu truyen len trong rong
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu đơn hàng không hợp lệ" });
            }

            try
            {
                // Buoc A: Tu dong khoi tao cau truc thuc the Don hang moi
                var newOrder = new Order
                {
                    OrderDate = DateTime.Now, // Tu dong lay ngay gio thuc te may tinh luc mua
                    CustomerId = input.CustomerId,
                    Status = 0,               // 0: Mac dinh don hang moi o trang thai "Cho xu ly"
                    Notes = input.Notes
                };

                // Buoc B: Them vao bang tam va chot luu xuong SQL Server
                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync(); // Ep he thong sinh ra ma ID Don hang tu dong tang

                // Buoc C: Tra ve ma thanh cong 201 Created va gui nguoc lai ma ID don hang vua tao
                return StatusCode(201, new {
                    message = "Đặt hàng thành công!",
                    orderId = newOrder.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý tạo đơn hàng ngầm", detail = ex.Message });
            }
        }
    }

    // LOP DTO TRUNG GIAN DE HUNG DU LIEU TU FRONTEND TRUYEN LEN
    // (Data Transfer Object - chi chua dung nhung truong Frontend gui len)
    public class OrderInputDTO
    {
        public int CustomerId { get; set; }
        public string? Notes { get; set; }
    }
}
