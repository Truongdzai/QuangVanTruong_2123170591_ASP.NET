// Nạp dữ liệu mẫu lần đầu (Buổi 2)
// Chạy tự động trong Program.cs sau Migrate()

using CMS.Data.Entities;

namespace CMS.Data;

public static class DbInitializer
{
    /// <summary>
    /// Thêm dữ liệu mẫu nếu database mới tạo (chưa có Category).
    /// Tránh chèn trùng khi restart app nhiều lần.
    /// </summary>
    public static void Seed(ApplicationDbContext context)
    {
        // Any(): có ít nhất 1 dòng → đã seed rồi → thoát
        if (context.Categories.Any())
            return;

        // --- BẢNG CATEGORIES (phải insert trước Posts vì có khóa ngoại CategoryId) ---
        var categories = new List<Category>
        {
            new() { Name = "Tin tức Công nghệ", Description = "Cập nhật xu hướng AI, IoT và lập trình." },
            new() { Name = "Đời sống du lịch", Description = "Kinh nghiệm phượt và các điểm đến hấp dẫn." },
            new() { Name = "Sức khỏe Thể thao", Description = "Các bài tập và chế độ ăn uống lành mạnh." },
            new() { Name = "Giáo dục Kỹ năng", Description = "Phương pháp học tập và kỹ năng mềm." },
            new() { Name = "Góc lập trình viên", Description = "Tài liệu ASP.NET Core và SQL Server." }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges(); // Ghi xuống SQL → Id tự tăng 1,2,3,4,5

        // --- BẢNG POSTS (CategoryId phải trùng Id đã có trong Categories) ---
        context.Posts.AddRange(
            new Post
            {
                Title = "Lộ trình học ASP.NET",
                Content = "Hướng dẫn chi tiết cho người mới bắt đầu...",
                ImageUrl = "https://via.placeholder.com/400x200?text=ASP.NET",
                CategoryId = 5, // Góc lập trình viên
                CreatedDate = new DateTime(2026, 4, 1)
            },
            new Post
            {
                Title = "Top 5 bãi biển đẹp",
                Content = "Những địa điểm không thể bỏ qua mùa hè này...",
                ImageUrl = "https://via.placeholder.com/400x200?text=Beach",
                CategoryId = 2,
                CreatedDate = new DateTime(2026, 4, 2)
            },
            new Post
            {
                Title = "Chạy bộ đúng cách",
                Content = "Lợi ích tuyệt vời của việc chạy bộ mỗi sáng...",
                ImageUrl = "https://via.placeholder.com/400x200?text=Running",
                CategoryId = 3,
                CreatedDate = new DateTime(2026, 4, 3)
            },
            new Post
            {
                Title = "AI và tương lai",
                Content = "Trí tuệ nhân tạo đang thay đổi cuộc sống...",
                ImageUrl = "https://via.placeholder.com/400x200?text=AI",
                CategoryId = 1,
                CreatedDate = new DateTime(2026, 4, 4)
            },
            new Post
            {
                Title = "Kỹ năng Teamwork",
                Content = "Cách phối hợp hiệu quả trong nhóm dự án...",
                ImageUrl = "https://via.placeholder.com/400x200?text=Team",
                CategoryId = 4,
                CreatedDate = new DateTime(2026, 4, 5)
            }
        );

        // --- BẢNG USERS (mật khẩu lưu thô — chỉ dùng học tập, Buổi 5 sẽ hash) ---
        context.Users.AddRange(
            new User { Username = "admin", PasswordHash = "123456", FullName = "Quản trị viên hệ thống", Role = "Admin" },
            new User { Username = "thai_gv", PasswordHash = "thai1969", FullName = "Nguyễn Cao Thái", Role = "Editor" },
            new User { Username = "sv_01", PasswordHash = "student1", FullName = "Nguyễn Văn A", Role = "User" },
            new User { Username = "sv_02", PasswordHash = "student2", FullName = "Trần Thị B", Role = "User" },
            new User { Username = "moderator", PasswordHash = "mod789", FullName = "Lê Văn C", Role = "Moderator" }
        );

        context.SaveChanges(); // Ghi Posts + Users
    }
}
