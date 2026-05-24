// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 4
// Ngay thuc hien: 23/03/2026
// Version: 1.4

using CMS.Data.Entities;

namespace CMS.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        // Chay truoc: doi anh ngoai (https://) sang anh local trong wwwroot/images/posts
        CapNhatAnhBaiViet(context);

        // Any() = da co du lieu -> khong seed lai lan 2
        if (context.Categories.Any())
            return;

        // --- BANG CATEGORIES (phai ghi truoc Posts vi Posts co khoa ngoai CategoryId) ---
        var categories = new List<Category>
        {
            new() { Name = "Tin tức Công nghệ",  Description = "Cập nhật xu hướng AI, IoT và lập trình."           },
            new() { Name = "Đời sống du lịch",    Description = "Kinh nghiệm phượt và các điểm đến hấp dẫn."        },
            new() { Name = "Sức khỏe Thể thao",   Description = "Các bài tập và chế độ ăn uống lành mạnh."          },
            new() { Name = "Giáo dục Kỹ năng",    Description = "Phương pháp học tập và kỹ năng mềm."               },
            new() { Name = "Góc lập trình viên",  Description = "Tài liệu ASP.NET Core và SQL Server."              }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges(); // Ghi xuong SQL -> Id tu tang 1,2,3,4,5

        // --- BANG POSTS: dung anh local tu wwwroot/images/posts (img1 -> img5) ---
        // img6.png con lai de admin tu upload bai moi
        context.Posts.AddRange(
            new Post
            {
                Title       = "Lộ trình học ASP.NET",
                Content     = "Hướng dẫn chi tiết cho người mới bắt đầu học ASP.NET Core từ cơ bản đến nâng cao.",
                ImageUrl    = "/images/posts/img1.png",
                CategoryId  = 5,   // Goc lap trinh vien
                CreatedDate = new DateTime(2026, 4, 1)
            },
            new Post
            {
                Title       = "Top 5 bãi biển đẹp",
                Content     = "Những địa điểm không thể bỏ qua mùa hè này, từ Phú Quốc đến Đà Nẵng.",
                ImageUrl    = "/images/posts/img2.png",
                CategoryId  = 2,   // Doi song du lich
                CreatedDate = new DateTime(2026, 4, 2)
            },
            new Post
            {
                Title       = "Chạy bộ đúng cách",
                Content     = "Lợi ích tuyệt vời của việc chạy bộ mỗi sáng và kỹ thuật chạy đúng không gây đau khớp.",
                ImageUrl    = "/images/posts/img3.png",
                CategoryId  = 3,   // Suc khoe The thao
                CreatedDate = new DateTime(2026, 4, 3)
            },
            new Post
            {
                Title       = "AI và tương lai",
                Content     = "Trí tuệ nhân tạo đang thay đổi cuộc sống như thế nào và cơ hội nghề nghiệp trong lĩnh vực AI.",
                ImageUrl    = "/images/posts/img4.png",
                CategoryId  = 1,   // Tin tuc Cong nghe
                CreatedDate = new DateTime(2026, 4, 4)
            },
            new Post
            {
                Title       = "Kỹ năng Teamwork",
                Content     = "Cách phối hợp hiệu quả trong nhóm dự án, giải quyết xung đột và đạt mục tiêu chung.",
                ImageUrl    = "/images/posts/img5.png",
                CategoryId  = 4,   // Giao duc Ky nang
                CreatedDate = new DateTime(2026, 4, 5)
            }
        );

        // --- BANG USERS (mat khau luu tho - chi dung hoc tap, Buoi 5 se hash BCrypt) ---
        context.Users.AddRange(
            new User { Username = "admin",     PasswordHash = "123456",   FullName = "Quản trị viên hệ thống", Role = "Admin"     },
            new User { Username = "thai_gv",   PasswordHash = "thai1969", FullName = "Nguyễn Cao Thái",        Role = "Editor"    },
            new User { Username = "sv_01",     PasswordHash = "student1", FullName = "Nguyễn Văn A",           Role = "User"      },
            new User { Username = "sv_02",     PasswordHash = "student2", FullName = "Trần Thị B",             Role = "User"      },
            new User { Username = "moderator", PasswordHash = "mod789",   FullName = "Lê Văn C",               Role = "Moderator" }
        );

        context.SaveChanges(); // Ghi Posts + Users xuong SQL
    }

    // Cap nhat ImageUrl tu URL ngoai (picsum.photos) sang anh local wwwroot/images/posts
    // Ham nay chay moi lan app khoi dong, tu bao gio khong con bai nao dung anh ngoai thi bo qua
    private static void CapNhatAnhBaiViet(ApplicationDbContext context)
    {
        // Ban do: ten bai viet -> duong dan anh local tuong doi
        var map = new Dictionary<string, string>
        {
            ["Lộ trình học ASP.NET"] = "/images/posts/img1.png",
            ["Top 5 bãi biển đẹp"]  = "/images/posts/img2.png",
            ["Chạy bộ đúng cách"]   = "/images/posts/img3.png",
            ["AI và tương lai"]      = "/images/posts/img4.png",
            ["Kỹ năng Teamwork"]     = "/images/posts/img5.png",
        };

        // Chi lay bai dang co ImageUrl bat dau bang https:// (anh ngoai)
        var baiCanSua = context.Posts
            .Where(p => p.ImageUrl != null && p.ImageUrl.StartsWith("https://"))
            .ToList();

        // Neu khong co bai nao can sua thi thoat luon
        if (!baiCanSua.Any()) return;

        bool daSua = false;
        foreach (var bai in baiCanSua)
        {
            // TryGetValue: tim ten bai trong ban do, neu co thi cap nhat ImageUrl
            if (map.TryGetValue(bai.Title, out var anhMoi))
            {
                bai.ImageUrl = anhMoi;
                daSua = true;
            }
        }

        // Chi goi SaveChanges khi thuc su co thay doi, tranh ghi DB thua
        if (daSua)
            context.SaveChanges();
    }
}
