namespace CMS.Data.Entities;

/// <summary>
/// Thực thể Người dùng quản trị (bảng Users) — khác bảng Customers (khách mua hàng).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;     // Tên đăng nhập
    public string PasswordHash { get; set; } = string.Empty; // Mật khẩu (Buổi 5: hash thật)
    public string FullName { get; set; } = string.Empty;     // Họ tên hiển thị
    public string Role { get; set; } = string.Empty;         // Admin, Editor, User...
}
