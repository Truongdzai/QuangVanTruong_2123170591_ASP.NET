// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 7 (cap nhat them seed du lieu san pham)
// Ngay thuc hien: 27/05/2026
// Version: 1.7

using CMS.Data.Entities;

namespace CMS.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        // Chay truoc: cap nhat anh bai viet tu URL ngoai sang local
        CapNhatAnhBaiViet(context);

        // ── Buoi 1-6: Seed danh muc bai viet, bai viet, nguoi dung ───────────
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Tin tức Công nghệ",  Description = "Cập nhật xu hướng AI, IoT và lập trình."           },
                new() { Name = "Đời sống du lịch",    Description = "Kinh nghiệm phượt và các điểm đến hấp dẫn."        },
                new() { Name = "Sức khỏe Thể thao",   Description = "Các bài tập và chế độ ăn uống lành mạnh."          },
                new() { Name = "Giáo dục Kỹ năng",    Description = "Phương pháp học tập và kỹ năng mềm."               },
                new() { Name = "Góc lập trình viên",  Description = "Tài liệu ASP.NET Core và SQL Server."              }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            context.Posts.AddRange(
                new Post { Title = "Lộ trình học ASP.NET",  Content = "Hướng dẫn chi tiết cho người mới bắt đầu học ASP.NET Core từ cơ bản đến nâng cao.", ImageUrl = "/images/posts/img1.png", CategoryId = 5, CreatedDate = new DateTime(2026, 4, 1) },
                new Post { Title = "Top 5 bãi biển đẹp",    Content = "Những địa điểm không thể bỏ qua mùa hè này, từ Phú Quốc đến Đà Nẵng.",          ImageUrl = "/images/posts/img2.png", CategoryId = 2, CreatedDate = new DateTime(2026, 4, 2) },
                new Post { Title = "Chạy bộ đúng cách",     Content = "Lợi ích tuyệt vời của việc chạy bộ mỗi sáng và kỹ thuật chạy đúng không gây đau khớp.", ImageUrl = "/images/posts/img3.png", CategoryId = 3, CreatedDate = new DateTime(2026, 4, 3) },
                new Post { Title = "AI và tương lai",        Content = "Trí tuệ nhân tạo đang thay đổi cuộc sống như thế nào và cơ hội nghề nghiệp trong lĩnh vực AI.", ImageUrl = "/images/posts/img4.png", CategoryId = 1, CreatedDate = new DateTime(2026, 4, 4) },
                new Post { Title = "Kỹ năng Teamwork",       Content = "Cách phối hợp hiệu quả trong nhóm dự án, giải quyết xung đột và đạt mục tiêu chung.", ImageUrl = "/images/posts/img5.png", CategoryId = 4, CreatedDate = new DateTime(2026, 4, 5) }
            );

            context.Users.AddRange(
                new User { Username = "admin",     PasswordHash = "123456",   FullName = "Quản trị viên hệ thống", Role = "Admin"     },
                new User { Username = "thai_gv",   PasswordHash = "thai1969", FullName = "Nguyễn Cao Thái",        Role = "Editor"    },
                new User { Username = "sv_01",     PasswordHash = "student1", FullName = "Nguyễn Văn A",           Role = "User"      },
                new User { Username = "sv_02",     PasswordHash = "student2", FullName = "Trần Thị B",             Role = "User"      },
                new User { Username = "moderator", PasswordHash = "mod789",   FullName = "Lê Văn C",               Role = "Moderator" }
            );

            context.SaveChanges();
        }

        // ── Buoi 7: Seed danh muc san pham thoi trang ───────────────────────
        if (!context.CategoriesProducts.Any())
        {
            context.CategoriesProducts.AddRange(
                new CategoryProduct { Name = "Casual", Description = "Trang phục thường ngày thoải mái, năng động" },
                new CategoryProduct { Name = "Formal", Description = "Trang phục lịch sự, phù hợp công sở"        },
                new CategoryProduct { Name = "Party",  Description = "Trang phục thời trang cho các buổi tiệc"     },
                new CategoryProduct { Name = "Gym",    Description = "Trang phục thể thao hiệu suất cao"           }
            );
            context.SaveChanges();
        }

        // ── Buoi 7: Seed san pham thoi trang SHOP.CO ────────────────────────
        if (!context.Products.Any())
        {
            // Lay Id danh muc vua them
            var casual = context.CategoriesProducts.First(c => c.Name == "Casual").Id;
            var formal = context.CategoriesProducts.First(c => c.Name == "Formal").Id;
            var party  = context.CategoriesProducts.First(c => c.Name == "Party").Id;
            var gym    = context.CategoriesProducts.First(c => c.Name == "Gym").Id;

            context.Products.AddRange(
                new Product
                {
                    Name              = "T-SHIRT WITH TAPE DETAILS",
                    Description       = "A classic T-shirt with modern tape details for a stylish, sporty look. Made from 100% premium cotton for maximum comfort.",
                    Price             = 120,
                    StockQuantity     = 50,
                    ImageUrl          = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = casual
                },
                new Product
                {
                    Name              = "SKINNY FIT JEANS",
                    Description       = "Sleek and modern skinny fit jeans that hug your curves in all the right places. Stretch denim for all-day comfort.",
                    Price             = 130,
                    StockQuantity     = 35,
                    ImageUrl          = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = formal
                },
                new Product
                {
                    Name              = "CHECKERED SHIRT",
                    Description       = "A timeless checkered pattern shirt perfect for casual and semi-formal occasions. Breathable cotton blend fabric.",
                    Price             = 240,
                    StockQuantity     = 28,
                    ImageUrl          = "https://images.unsplash.com/photo-1506629082955-511b1aa562c8?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = casual
                },
                new Product
                {
                    Name              = "SLEEVE STRIPED T-SHIRT",
                    Description       = "Bold sleeve stripes make this T-shirt a standout piece. Lightweight, breathable, and machine washable.",
                    Price             = 180,
                    StockQuantity     = 60,
                    ImageUrl          = "https://images.unsplash.com/photo-1576566588028-4147f3842f27?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = casual
                },
                new Product
                {
                    Name              = "VERTICAL STRIPED SHIRT",
                    Description       = "Elegant vertical stripes create a slimming silhouette. Perfect for the office or a smart-casual evening.",
                    Price             = 212,
                    StockQuantity     = 42,
                    ImageUrl          = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = formal
                },
                new Product
                {
                    Name              = "COURAGE GRAPHIC T-SHIRT",
                    Description       = "Wear your courage on your sleeve with this bold graphic tee. Premium soft cotton, relaxed fit.",
                    Price             = 145,
                    StockQuantity     = 70,
                    ImageUrl          = "https://images.unsplash.com/photo-1503341504253-dff4815485f1?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = casual
                },
                new Product
                {
                    Name              = "LOOSE FIT BERMUDA SHORTS",
                    Description       = "Relaxed Bermuda shorts for the beach, gym, or weekend errands. Elastic waistband for a comfortable fit.",
                    Price             = 80,
                    StockQuantity     = 90,
                    ImageUrl          = "https://images.unsplash.com/photo-1591195853828-11db59a44f43?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = gym
                },
                new Product
                {
                    Name              = "FADED SKINNY JEANS",
                    Description       = "Vintage-inspired faded finish meets modern skinny cut. The perfect pair of jeans for any occasion.",
                    Price             = 210,
                    StockQuantity     = 38,
                    ImageUrl          = "https://images.unsplash.com/photo-1549058595-1bc6b0c72a2e?w=295&h=393&fit=crop&q=80",
                    CategoryProductId = formal
                }
            );
            context.SaveChanges();
        }
    }

    // Cap nhat ImageUrl bai viet tu URL ngoai sang anh local trong wwwroot
    private static void CapNhatAnhBaiViet(ApplicationDbContext context)
    {
        var map = new Dictionary<string, string>
        {
            ["Lộ trình học ASP.NET"] = "/images/posts/img1.png",
            ["Top 5 bãi biển đẹp"]  = "/images/posts/img2.png",
            ["Chạy bộ đúng cách"]   = "/images/posts/img3.png",
            ["AI và tương lai"]      = "/images/posts/img4.png",
            ["Kỹ năng Teamwork"]     = "/images/posts/img5.png",
        };

        var baiCanSua = context.Posts
            .Where(p => p.ImageUrl != null && p.ImageUrl.StartsWith("https://"))
            .ToList();

        if (!baiCanSua.Any()) return;

        bool daSua = false;
        foreach (var bai in baiCanSua)
        {
            if (map.TryGetValue(bai.Title, out var anhMoi))
            {
                bai.ImageUrl = anhMoi;
                daSua = true;
            }
        }

        if (daSua) context.SaveChanges();
    }
}
