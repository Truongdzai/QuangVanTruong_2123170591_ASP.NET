using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// CHƯƠNG TRÌNH FLASH SALE (bảng FlashSales) — mục "5. Khuyến mãi &amp; Marketing".
/// Giảm giá sản phẩm TỰ ĐỘNG theo khung giờ; Backend luôn kiểm tra
/// thời gian SERVER (test case 9: hết 12:00:00 là giá quay về gốc,
/// không tin tưởng đồng hồ của Frontend).
/// </summary>
public class FlashSale
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên chương trình không được để trống!")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Thời điểm bắt đầu áp giá sale (giờ server)</summary>
    public DateTime StartTime { get; set; }

    /// <summary>Thời điểm kết thúc — qua giây này giá tự quay về giá gốc</summary>
    public DateTime EndTime { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public virtual ICollection<FlashSaleItem> Items { get; set; } = new List<FlashSaleItem>();
}

/// <summary>
/// Sản phẩm nằm trong 1 chương trình Flash Sale, kèm giá sale riêng.
/// </summary>
public class FlashSaleItem
{
    [Key]
    public int Id { get; set; }

    public int FlashSaleId { get; set; }
    public int ProductId { get; set; }

    /// <summary>Giá bán trong khung giờ sale (thay cho giá gốc)</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalePrice { get; set; }

    /// <summary>Giới hạn số lượng bán giá sale (0 = không giới hạn)</summary>
    public int QuantityLimit { get; set; }

    /// <summary>Đã bán được bao nhiêu trong chương trình</summary>
    public int SoldCount { get; set; }

    [ForeignKey(nameof(FlashSaleId))]
    public virtual FlashSale? FlashSale { get; set; }

    [ForeignKey(nameof(ProductId))]
    public virtual Product? Product { get; set; }
}
