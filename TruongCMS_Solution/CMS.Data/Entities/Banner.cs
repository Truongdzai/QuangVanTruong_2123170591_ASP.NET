using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// Buổi 11 — Banner động cho HeroSlider trang chủ (bảng Banners).
/// Admin quản lý ảnh + tiêu đề + link; FrontEnd đọc qua GET /api/banners.
/// </summary>
public class Banner
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề banner không được để trống!")]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;   // Dòng chữ lớn trên slide

    [StringLength(300)]
    public string? Subtitle { get; set; }               // Mô tả ngắn dưới tiêu đề

    [Required(ErrorMessage = "Banner phải có ảnh (upload hoặc dán link)!")]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Bấm nút trên slide sẽ đi đâu (vd /products?category=2)</summary>
    [StringLength(300)]
    public string? LinkUrl { get; set; }

    [StringLength(40)]
    public string? ButtonText { get; set; }             // Chữ trên nút (mặc định "Mua ngay")

    /// <summary>Thứ tự hiển thị: nhỏ trước, lớn sau</summary>
    public int SortOrder { get; set; }

    /// <summary>Tắt = ẩn khỏi trang chủ nhưng không phải xóa</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
