// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Nang cap theo BAO CAO NGHIEN CUU: van chuyen (muc 2 — Logistics)
// Webhook trang thai giao hang IDEMPOTENT + bao gia phi ship

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using CMS.Data.Entities;
using CMS.Backend.Services;
using System.ComponentModel.DataAnnotations;

namespace CMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShippingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IShippingService _shipping;
        private readonly IOrderWorkflowService _workflow;
        private readonly IConfiguration _config;
        private readonly ILogger<ShippingController> _logger;

        public ShippingController(
            ApplicationDbContext context,
            IShippingService shipping,
            IOrderWorkflowService workflow,
            IConfiguration config,
            ILogger<ShippingController> logger)
        {
            _context = context;
            _shipping = shipping;
            _workflow = workflow;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/shipping/quote — gio hang hoi phi van chuyen truoc khi dat
        /// (don tu nguong FreeThreshold duoc mien phi).
        /// </summary>
        [HttpPost("quote")]
        public IActionResult Quote([FromBody] ShippingQuoteDTO input)
        {
            decimal phi = _shipping.TinhPhiShip(input.Subtotal, input.ItemCount);
            return Ok(new
            {
                fee = phi,
                freeThreshold = decimal.TryParse(_config["Shipping:FreeThreshold"], out var n) ? n : 500
            });
        }

        /// <summary>
        /// POST /api/shipping/callback — WEBHOOK don vi van chuyen bao trang thai
        /// (GHTK goi POST den callback URL; ta phai tra HTTP 200 de ho khong retry).
        /// IDEMPOTENT: callback den TRUNG (hang retry) -> trang thai da dung roi thi
        /// tra 200 "da ghi nhan", KHONG cap nhat lan 2. Bao mat bang X-Callback-Token.
        /// </summary>
        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] ShippingCallbackDTO input)
        {
            // Xac thuc webhook: header token phai khop cau hinh (chong gia mao)
            string tokenCauHinh = _config["Shipping:CallbackToken"] ?? "GHTK-DEMO-TOKEN";
            if (!Request.Headers.TryGetValue("X-Callback-Token", out var token)
                || token != tokenCauHinh)
            {
                _logger.LogWarning("[SHIPPING-WEBHOOK] Token sai tu IP {IP}",
                    HttpContext.Connection.RemoteIpAddress);
                return Unauthorized(new { message = "Sai callback token." });
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.TrackingCode == input.TrackingCode);

            if (order == null)
                return NotFound(new { message = $"Không có đơn nào mang mã vận đơn {input.TrackingCode}." });

            // Map trang thai hang van chuyen -> trang thai noi bo
            int? trangThaiMoi = input.Status?.ToLower() switch
            {
                "picked" or "shipping" => OrderStatusFlow.DangGiao,
                "delivered"            => OrderStatusFlow.ThanhCong,
                "returned"             => OrderStatusFlow.HoanTra,
                _                      => null
            };

            if (trangThaiMoi == null)
                return BadRequest(new { message = $"Trạng thái '{input.Status}' không được hỗ trợ." });

            // IDEMPOTENT: hang giao retry cung mot trang thai -> tra 200 luon
            if (order.Status == trangThaiMoi.Value)
                return Ok(new { message = "Đã ghi nhận trước đó (idempotent).", orderId = order.Id });

            var kq = await _workflow.DoiTrangThaiAsync(order.Id, trangThaiMoi.Value,
                $"Webhook {order.ShippingProvider}: {input.Status} — {input.Note}");

            if (!kq.ThanhCong)
            {
                // Buoc chuyen khong hop le (vd bao delivered khi don da huy):
                // van tra 200 de hang van chuyen khong retry vo han, kem ghi chu
                _logger.LogWarning("[SHIPPING-WEBHOOK] Bo qua: {Loi}", kq.Loi);
                return Ok(new { message = $"Bỏ qua: {kq.Loi}", orderId = order.Id });
            }

            return Ok(new { message = "Cập nhật trạng thái thành công.", orderId = order.Id });
        }
    }

    public class ShippingQuoteDTO
    {
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
    }

    public class ShippingCallbackDTO
    {
        [Required]
        public string TrackingCode { get; set; } = string.Empty;

        /// <summary>picked / shipping / delivered / returned</summary>
        [Required]
        public string Status { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}
