using System.ComponentModel.DataAnnotations;

namespace CMS.Data.Entities;

/// <summary>
/// Thực thể Bài viết (bảng Posts).
/// Cột Title & Content là NOT NULL dưới SQL — bắt buộc nhập để tránh
/// DbUpdateException "Cannot insert the value NULL" khi lưu (kiểm tra ModelState ở Controller).
/// </summary>
public class Post
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết!")]
    public string Title { get; set; } = string.Empty;       // Tiêu đề hiển thị trên card

    [Required(ErrorMessage = "Vui lòng nhập nội dung bài viết!")]
    public string Content { get; set; } = string.Empty;     // Nội dung đầy đủ (trang Details)

    public string? ImageUrl { get; set; }                   // URL ảnh đại diện
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Khóa ngoại -> bảng Categories
    [Required(ErrorMessage = "Vui lòng chọn chuyên mục cho bài viết!")]
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }           // Đối tượng danh mục liên kết
}
