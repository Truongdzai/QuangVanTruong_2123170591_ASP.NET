namespace CMS.Data.Entities;

/// <summary>
/// Thực thể Bài viết (bảng Posts).
/// </summary>
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;       // Tiêu đề hiển thị trên card
    public string Content { get; set; } = string.Empty;     // Nội dung đầy đủ (trang Details)
    public string? ImageUrl { get; set; }                   // URL ảnh đại diện
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Khóa ngoại -> bảng Categories
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }           // Đối tượng danh mục liên kết
}
