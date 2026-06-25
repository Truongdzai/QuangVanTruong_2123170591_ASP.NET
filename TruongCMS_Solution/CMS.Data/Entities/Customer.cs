using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// Khách hàng mua hàng (bảng Customers) — không phải User quản trị.
/// </summary>
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress] // Kiểm tra định dạng email khi validate form (Buổi sau)
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Address { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty; // Buổi 9: luôn lưu dạng hash SHA256+Salt

    // Buổi 9 — Quên mật khẩu (Tiêu chí 46): mã xác nhận + hạn dùng (15 phút)
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    // Buổi 11 — true = tài khoản tự sinh khi khách vãng lai đặt hàng (chưa đăng ký thật).
    // Khi khách đăng ký bằng email này thì "kích hoạt" tài khoản thay vì báo trùng email.
    public bool IsGuest { get; set; }

    /// <summary>
    /// ĐIỂM TÍCH LŨY (mục 4 báo cáo — Loyalty): mỗi đơn Thành công cộng điểm
    /// theo giá trị đơn; điểm quyết định hạng thành viên Bạc / Vàng / Kim cương.
    /// </summary>
    public int LoyaltyPoints { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    /// <summary>Hạng thành viên suy ra từ điểm tích lũy (không lưu DB)</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string MembershipTier => LoyaltyPoints >= 2000 ? "Kim cương"
                                  : LoyaltyPoints >= 500  ? "Vàng"
                                  : "Bạc";
}
