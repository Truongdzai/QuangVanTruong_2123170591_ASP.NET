using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// LIÊN KẾT FOOTER ĐỘNG (bảng FooterLinks) — admin tự thêm/sửa các cột link
/// ở chân trang mà không phải sửa code React. Nhóm theo `GroupHeading`.
/// </summary>
public class FooterLink
{
    [Key]
    public int Id { get; set; }

    /// <summary>Tiêu đề cột, VD "CÔNG TY", "HỖ TRỢ"</summary>
    [Required(ErrorMessage = "Tiêu đề cột không được để trống!")]
    [StringLength(60)]
    public string GroupHeading { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nhãn hiển thị không được để trống!")]
    [StringLength(80)]
    public string Label { get; set; } = string.Empty;

    /// <summary>Đường dẫn: nội bộ "/about" hoặc ngoài "https://..."</summary>
    [Required(ErrorMessage = "Đường dẫn không được để trống!")]
    [StringLength(300)]
    public string Url { get; set; } = "/";

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// CẤU HÌNH SITE dạng key-value (bảng SiteSettings) — mô tả footer, link mạng
/// xã hội, dòng bản quyền... Admin sửa giá trị, React đọc qua API.
/// </summary>
public class SiteSetting
{
    [Key]
    [StringLength(60)]
    public string Key { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Value { get; set; }
}
