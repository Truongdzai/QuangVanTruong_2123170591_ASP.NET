using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Data.Entities;

/// <summary>
/// REFRESH TOKEN của khách hàng (bảng RefreshTokens) — test case 15:
/// Access Token JWT hết hạn (15 phút) thì Frontend âm thầm dùng Refresh Token
/// (7 ngày) đổi lấy token mới, khách KHÔNG phải đăng nhập lại.
/// Token cũ bị thu hồi (xoay vòng) mỗi lần refresh để chống đánh cắp.
/// </summary>
public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public int CustomerId { get; set; }

    /// <summary>Chuỗi ngẫu nhiên 64 byte (Base64) — duy nhất toàn hệ thống</summary>
    [Required, StringLength(128)]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>Khác null = token đã bị thu hồi (logout / đã xoay vòng)</summary>
    public DateTime? RevokedDate { get; set; }

    [NotMapped]
    public bool IsUsable => RevokedDate == null && ExpiryDate > DateTime.Now;

    [ForeignKey(nameof(CustomerId))]
    public virtual Customer? Customer { get; set; }
}
