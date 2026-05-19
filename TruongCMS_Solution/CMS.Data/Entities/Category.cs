namespace CMS.Data.Entities;

/// <summary>
/// Thực thể Danh mục tin (bảng Categories).
/// Quan hệ 1-n: 1 danh mục có nhiều bài viết (Post).
/// </summary>
public class Category
{
    public int Id { get; set; }                      // Khóa chính, Identity trong SQL
    public string Name { get; set; } = string.Empty; // Tên danh mục (vd: Tin Công nghệ)
    public string? Description { get; set; }         // Mô tả ngắn (nullable)

    // Navigation property: EF dùng để Include() / join bảng Posts
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
