using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// Buổi 11 — Phiếu NHẬP KHO từ các hãng (bảng StockReceipts).
/// Mỗi phiếu = nhập 1 sản phẩm từ 1 hãng; lưu xong hệ thống tự CỘNG tồn kho.
/// </summary>
public class StockReceipt
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên hãng cung cấp không được để trống!")]
    [StringLength(120)]
    public string SupplierName { get; set; } = string.Empty;  // VD: Coolmate, Nike, Adidas

    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }

    /// <summary>Số lượng nhập về (cộng vào StockQuantity)</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0!")]
    public int Quantity { get; set; }

    /// <summary>Giá nhập 1 đơn vị từ hãng (để đối chiếu lãi/lỗ)</summary>
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }

    public DateTime ReceiptDate { get; set; } = DateTime.Now;

    [StringLength(300)]
    public string? Notes { get; set; }
}
