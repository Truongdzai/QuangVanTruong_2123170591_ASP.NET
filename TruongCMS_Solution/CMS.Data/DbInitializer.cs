// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 9 (bao mat mat khau + anh danh muc san pham)
// Ngay thuc hien: 11/06/2026
// Version: 1.9

using CMS.Data.Entities;

namespace CMS.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        // Chay truoc: cap nhat anh bai viet tu URL ngoai sang local
        CapNhatAnhBaiViet(context);

        // Buoi 9 (Tieu chi 33): nang cap mat khau tho cu trong DB thanh hash SHA256+Salt
        NangCapMatKhauThanhHash(context);

        // Buoi 9 (Tieu chi 38): bo sung anh dai dien cho danh muc san pham da co
        CapNhatAnhDanhMucSanPham(context);

        // Buoi 10: gan Mau sac & Kich co cho san pham chua co (phuc vu LOC THAT)
        CapNhatMauSizeSanPham(context);

        // ── NÂNG CẤP THEO BÁO CÁO NGHIÊN CỨU (deep-research-report) ─────────
        SeedThuongHieu(context);        // Muc 1: thuong hieu / nhan hang
        SeedBienTheSanPham(context);    // Muc 1: bien the SKU (Mau × Size)
        SeedFlashSale(context);         // Muc 5: flash sale theo khung gio
        SeedMaGiamGiaNangCao(context);  // Test case 7-8: don toi thieu + 1 lan/khach
        SeedTaiKhoanNoiBo(context);     // Muc 4: RBAC — nhan vien kho / ke toan / CSKH
        SeedFooter(context);            // Footer dong: cot link + cau hinh site

        // ── Buoi 11: Seed banner dong cho HeroSlider trang chu ──────────────
        if (!context.Banners.Any())
        {
            context.Banners.AddRange(
                new Banner
                {
                    Title      = "BỘ SƯU TẬP HÈ 2026",
                    Subtitle   = "Khám phá những thiết kế mới nhất — chất liệu thoáng mát, phong cách trẻ trung cho mùa hè năng động.",
                    ImageUrl   = "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=900&h=600&fit=crop&q=80",
                    LinkUrl    = "/products?filter=new",
                    ButtonText = "Khám phá ngay",
                    SortOrder  = 1,
                    IsActive   = true
                },
                new Banner
                {
                    Title      = "GIẢM 20% CHO ĐƠN ĐẦU TIÊN",
                    Subtitle   = "Đăng ký tài khoản hôm nay để nhận ngay mã giảm giá chào mừng — áp dụng cho mọi sản phẩm.",
                    ImageUrl   = "https://images.unsplash.com/photo-1483985988355-763728e1935b?w=900&h=600&fit=crop&q=80",
                    LinkUrl    = "/register",
                    ButtonText = "Đăng ký ngay",
                    SortOrder  = 2,
                    IsActive   = true
                }
            );
            context.SaveChanges();
        }

        // ── Buoi 11: Seed ma giam gia cong khai SHOPCO (-20%) ───────────────
        if (!context.DiscountCodes.Any())
        {
            context.DiscountCodes.Add(new DiscountCode
            {
                Code        = "SHOPCO",
                Description = "Mã sale công khai của shop — giảm 20% mọi đơn hàng",
                Percent     = 20,
                MaxUses     = 0,    // khong gioi han
                IsActive    = true
            });
            context.SaveChanges();
        }

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

            // Buoi 9: Content luu dang HTML (san pham cua CKEditor) — FrontEnd render bang dangerouslySetInnerHTML
            context.Posts.AddRange(
                new Post { Title = "Lộ trình học ASP.NET",  Content = "<p><strong>ASP.NET Core</strong> là framework web đa nền tảng của Microsoft.</p><p>Hướng dẫn chi tiết cho người mới bắt đầu học ASP.NET Core từ <em>cơ bản đến nâng cao</em>: C#, MVC, Entity Framework Core rồi đến Web API.</p><ul><li>Tuần 1–2: Ngôn ngữ C#</li><li>Tuần 3–4: ASP.NET Core MVC</li><li>Tuần 5–6: EF Core &amp; SQL Server</li><li>Tuần 7–8: Web API + ReactJS</li></ul>", ImageUrl = "/images/posts/img1.png", CategoryId = 5, CreatedDate = new DateTime(2026, 4, 1) },
                new Post { Title = "Top 5 bãi biển đẹp",    Content = "<p>Những địa điểm <strong>không thể bỏ qua</strong> mùa hè này, từ Phú Quốc đến Đà Nẵng.</p><p>Mỗi bãi biển mang một vẻ đẹp riêng: cát trắng, nước trong xanh và hải sản tươi ngon đang chờ bạn khám phá.</p>",          ImageUrl = "/images/posts/img2.png", CategoryId = 2, CreatedDate = new DateTime(2026, 4, 2) },
                new Post { Title = "Chạy bộ đúng cách",     Content = "<p>Lợi ích tuyệt vời của việc <strong>chạy bộ mỗi sáng</strong> và kỹ thuật chạy đúng không gây đau khớp.</p><p style=\"text-align:center;\"><em>Hãy bắt đầu từ 15 phút mỗi ngày!</em></p>", ImageUrl = "/images/posts/img3.png", CategoryId = 3, CreatedDate = new DateTime(2026, 4, 3) },
                new Post { Title = "AI và tương lai",        Content = "<p><strong>Trí tuệ nhân tạo</strong> đang thay đổi cuộc sống như thế nào và cơ hội nghề nghiệp trong lĩnh vực AI.</p><p>Từ trợ lý ảo đến xe tự lái, AI len lỏi vào mọi ngóc ngách của đời sống hiện đại.</p>", ImageUrl = "/images/posts/img4.png", CategoryId = 1, CreatedDate = new DateTime(2026, 4, 4) },
                new Post { Title = "Kỹ năng Teamwork",       Content = "<p>Cách phối hợp hiệu quả trong nhóm dự án, <em>giải quyết xung đột</em> và đạt mục tiêu chung.</p><p>Giao tiếp rõ ràng và phân chia công việc hợp lý là chìa khóa thành công của mọi đội nhóm.</p>", ImageUrl = "/images/posts/img5.png", CategoryId = 4, CreatedDate = new DateTime(2026, 4, 5) }
            );

            // Buoi 9 (Tieu chi 33): KHONG luu mat khau tho — bam SHA256+Salt truoc khi seed
            context.Users.AddRange(
                new User { Username = "admin",     PasswordHash = PasswordHasher.Hash("123456"),   FullName = "Quản trị viên hệ thống", Role = "Admin"     },
                new User { Username = "thai_gv",   PasswordHash = PasswordHasher.Hash("thai1969"), FullName = "Nguyễn Cao Thái",        Role = "Editor"    },
                new User { Username = "sv_01",     PasswordHash = PasswordHasher.Hash("student1"), FullName = "Nguyễn Văn A",           Role = "User"      },
                new User { Username = "sv_02",     PasswordHash = PasswordHasher.Hash("student2"), FullName = "Trần Thị B",             Role = "User"      },
                new User { Username = "moderator", PasswordHash = PasswordHasher.Hash("mod789"),   FullName = "Lê Văn C",               Role = "Moderator" }
            );

            context.SaveChanges();
        }

        // ── Buoi 7: Seed danh muc san pham thoi trang ───────────────────────
        if (!context.CategoriesProducts.Any())
        {
            context.CategoriesProducts.AddRange(
                new CategoryProduct { Name = "Casual", Description = "Trang phục thường ngày thoải mái, năng động", ImageUrl = "https://images.unsplash.com/photo-1434389677669-e08b4cac3105?w=400&h=450&fit=crop&q=80" },
                new CategoryProduct { Name = "Formal", Description = "Trang phục lịch sự, phù hợp công sở",        ImageUrl = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=640&h=230&fit=crop&q=80" },
                new CategoryProduct { Name = "Party",  Description = "Trang phục thời trang cho các buổi tiệc",     ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=230&fit=crop&q=80" },
                new CategoryProduct { Name = "Gym",    Description = "Trang phục thể thao hiệu suất cao",           ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400&h=230&fit=crop&q=80" }
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

    // Buoi 9 (Tieu chi 33): quet User/Customer con luu mat khau tho -> bam lai bang SHA256+Salt
    private static void NangCapMatKhauThanhHash(ApplicationDbContext context)
    {
        bool daSua = false;

        foreach (var user in context.Users.Where(u => u.PasswordHash != "").ToList())
        {
            if (!PasswordHasher.IsHashed(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.Hash(user.PasswordHash);
                daSua = true;
            }
        }

        foreach (var kh in context.Customers.Where(c => c.Password != "").ToList())
        {
            if (!PasswordHasher.IsHashed(kh.Password))
            {
                kh.Password = PasswordHasher.Hash(kh.Password);
                daSua = true;
            }
        }

        if (daSua) context.SaveChanges();
    }

    // Buoi 10: san pham chua co Mau/Size -> gan bo gia tri xoay vong theo Id
    // de tinh nang LOC THAT theo mau & kich co demo duoc tren moi database cu.
    private static void CapNhatMauSizeSanPham(ApplicationDbContext context)
    {
        // Bang mau trung khop voi cac o tron o sidebar FrontEnd
        string[] bangMau =
        {
            "#00C12B", "#F50606", "#F5DD06", "#F57906", "#06CAF5",
            "#063AF5", "#7D06F5", "#F506A4", "#FFFFFF", "#000000"
        };

        // Cac bo size pho bien, xoay vong theo Id san pham
        string[] boSize =
        {
            "Small,Medium,Large",
            "Medium,Large,X-Large",
            "X-Small,Small,Medium",
            "Large,X-Large,XX-Large"
        };

        var thieuThuocTinh = context.Products
            .Where(p => p.Colors == null || p.Colors == "" || p.Sizes == null || p.Sizes == "")
            .ToList();

        if (!thieuThuocTinh.Any()) return;

        foreach (var sp in thieuThuocTinh)
        {
            if (string.IsNullOrEmpty(sp.Colors))
            {
                // Moi san pham co 3 mau, chon lech nhau theo Id de da dang
                sp.Colors = string.Join(",",
                    bangMau[sp.Id % 10],
                    bangMau[(sp.Id + 3) % 10],
                    bangMau[(sp.Id + 6) % 10]);
            }

            if (string.IsNullOrEmpty(sp.Sizes))
            {
                sp.Sizes = boSize[sp.Id % boSize.Length];
            }
        }

        context.SaveChanges();
    }

    // Buoi 9 (Tieu chi 38): danh muc san pham cu chua co anh -> gan anh dai dien mac dinh
    private static void CapNhatAnhDanhMucSanPham(ApplicationDbContext context)
    {
        var anhTheoTen = new Dictionary<string, string>
        {
            ["Casual"] = "https://images.unsplash.com/photo-1434389677669-e08b4cac3105?w=400&h=450&fit=crop&q=80",
            ["Formal"] = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=640&h=230&fit=crop&q=80",
            ["Party"]  = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=400&h=230&fit=crop&q=80",
            ["Gym"]    = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400&h=230&fit=crop&q=80",
        };

        var thieuAnh = context.CategoriesProducts
            .Where(c => c.ImageUrl == null || c.ImageUrl == "")
            .ToList();

        if (!thieuAnh.Any()) return;

        foreach (var dm in thieuAnh)
        {
            dm.ImageUrl = anhTheoTen.TryGetValue(dm.Name, out var anh)
                ? anh
                : "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=400&h=300&fit=crop&q=80";
        }
        context.SaveChanges();
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

    // ════════════════ SEED THEO BÁO CÁO NGHIÊN CỨU CHUYÊN SÂU ════════════════

    // Muc 1 bao cao: THUONG HIEU — tao 5 nhan hang va gan san pham xoay vong
    private static void SeedThuongHieu(ApplicationDbContext context)
    {
        if (!context.Brands.Any())
        {
            context.Brands.AddRange(
                new Brand { Name = "VERSACE",      Description = "Thời trang cao cấp Ý" },
                new Brand { Name = "ZARA",         Description = "Thời trang nhanh Tây Ban Nha" },
                new Brand { Name = "GUCCI",        Description = "Nhà mốt xa xỉ Ý" },
                new Brand { Name = "PRADA",        Description = "Thương hiệu xa xỉ Milan" },
                new Brand { Name = "Calvin Klein", Description = "Phong cách tối giản Mỹ" }
            );
            context.SaveChanges();
        }

        // San pham chua co thuong hieu -> gan xoay vong theo Id
        var brandIds = context.Brands.OrderBy(b => b.Id).Select(b => b.Id).ToArray();
        if (brandIds.Length == 0) return;

        var chuaCoBrand = context.Products.Where(p => p.BrandId == null).ToList();
        if (!chuaCoBrand.Any()) return;

        foreach (var sp in chuaCoBrand)
            sp.BrandId = brandIds[sp.Id % brandIds.Length];
        context.SaveChanges();
    }

    // Muc 1 bao cao: BIEN THE SKU — sinh tu CSV Colors × Sizes cua tung san pham.
    //  - Test case 1: moi MAU co anh rieng (dung tham so imgix cua Unsplash)
    //  - Test case 2: bien the dau tien cua san pham Id chia het cho 3 -> ton kho 0
    //  - Test case 3: size cang lon gia cang cao (+5% moi nac size)
    private static void SeedBienTheSanPham(ApplicationDbContext context)
    {
        if (context.ProductVariants.Any()) return;

        var sanPhams = context.Products.ToList();
        string[] thuTuSize = { "X-Small", "Small", "Medium", "Large", "X-Large", "XX-Large" };

        foreach (var sp in sanPhams)
        {
            var mauList  = (sp.Colors ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var sizeList = (sp.Sizes  ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (mauList.Length == 0 || sizeList.Length == 0) continue;

            int tongTon = Math.Max(sp.StockQuantity, mauList.Length * sizeList.Length);
            int tonMoiBienThe = Math.Max(1, tongTon / (mauList.Length * sizeList.Length));
            int demBienThe = 0;
            int tongThucTe = 0;

            for (int m = 0; m < mauList.Length; m++)
            {
                // Anh rieng theo mau: bien doi anh goc bang tham so imgix
                // (sat=-100 -> trang den; hue xoay mau) de demo "doi mau -> doi anh"
                string? anhTheoMau = sp.ImageUrl == null ? null : m switch
                {
                    0 => sp.ImageUrl,                       // mau 1: anh goc
                    1 => sp.ImageUrl + "&sat=-100",         // mau 2: den trang
                    _ => sp.ImageUrl + $"&hue={m * 60}&sat=-30" // mau 3+: xoay tong mau
                };

                for (int s = 0; s < sizeList.Length; s++)
                {
                    demBienThe++;

                    // Test case 2: tao san 1 bien the HET HANG de demo nut "Het hang"
                    int ton = (demBienThe == 1 && sp.Id % 3 == 0) ? 0 : tonMoiBienThe;
                    tongThucTe += ton;

                    // Test case 3: size lon hon gia cao hon 5% moi nac
                    int bacSize = Array.IndexOf(thuTuSize, sizeList[s]);
                    decimal? giaRieng = bacSize > 1
                        ? Math.Round(sp.Price * (1 + 0.05m * (bacSize - 1)), 0)
                        : null; // size nho dung gia goc

                    context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId     = sp.Id,
                        Sku           = $"SP{sp.Id:D3}-{ColorHelper.TenTiengViet(mauList[m]).Replace(" ", "")}-{sizeList[s]}",
                        Color         = mauList[m],
                        ColorName     = ColorHelper.TenTiengViet(mauList[m]),
                        Size          = sizeList[s],
                        PriceOverride = giaRieng,
                        StockQuantity = ton,
                        ImageUrl      = anhTheoMau,
                        IsActive      = true
                    });
                }
            }

            // Dong bo ton kho tong cua Product = tong cac bien the
            sp.StockQuantity = tongThucTe;
        }

        context.SaveChanges();
    }

    // Muc 5 bao cao: FLASH SALE dang chay (3 ngay ke tu hom nay) — 3 san pham giam ~30%
    private static void SeedFlashSale(ApplicationDbContext context)
    {
        if (context.FlashSales.Any()) return;

        var idSanPham = context.Products.OrderBy(p => p.Id).Select(p => new { p.Id, p.Price })
                               .Take(3).ToList();
        if (idSanPham.Count == 0) return;

        var sale = new FlashSale
        {
            Name      = "FLASH SALE CUỐI TUẦN",
            StartTime = DateTime.Today,
            EndTime   = DateTime.Today.AddDays(3),
            IsActive  = true
        };
        context.FlashSales.Add(sale);
        context.SaveChanges();

        foreach (var sp in idSanPham)
        {
            context.FlashSaleItems.Add(new FlashSaleItem
            {
                FlashSaleId   = sale.Id,
                ProductId     = sp.Id,
                SalePrice     = Math.Round(sp.Price * 0.7m, 0), // giam 30%
                QuantityLimit = 0
            });
        }
        context.SaveChanges();
    }

    // Test case 7-8: ma giam gia co DON TOI THIEU + GIOI HAN 1 LAN/KHACH
    private static void SeedMaGiamGiaNangCao(ApplicationDbContext context)
    {
        if (context.DiscountCodes.Any(d => d.Code == "SHOPCO500")) return;

        context.DiscountCodes.Add(new DiscountCode
        {
            Code               = "SHOPCO500",
            Description        = "Giảm 10% cho đơn hàng từ 500 — mỗi khách dùng 1 lần",
            Percent            = 10,
            MinOrderAmount     = 500,
            MaxUsesPerCustomer = 1,
            MaxUses            = 0,
            IsActive           = true
        });
        context.SaveChanges();
    }

    // Muc 4 bao cao: RBAC — tai khoan noi bo theo vai tro van hanh shop
    private static void SeedTaiKhoanNoiBo(ApplicationDbContext context)
    {
        var taiKhoanMau = new (string Username, string Password, string FullName, string Role)[]
        {
            ("nhanvien_kho", "kho123456",   "Nhân viên đóng gói", "Staff"),
            ("ketoan",       "ketoan12345", "Kế toán shop",        "Accountant"),
            ("cskh",         "cskh123456",  "Nhân viên CSKH",      "Support"),
        };

        bool daThem = false;
        foreach (var tk in taiKhoanMau)
        {
            if (context.Users.Any(u => u.Username == tk.Username)) continue;
            context.Users.Add(new User
            {
                Username     = tk.Username,
                PasswordHash = PasswordHasher.Hash(tk.Password),
                FullName     = tk.FullName,
                Role         = tk.Role
            });
            daThem = true;
        }
        if (daThem) context.SaveChanges();
    }

    // FOOTER DONG: seed cac cot link + cau hinh site (admin sua lai trong trang quan tri)
    private static void SeedFooter(ApplicationDbContext context)
    {
        if (!context.FooterLinks.Any())
        {
            var links = new List<FooterLink>();
            void Add(string nhom, params (string Label, string Url)[] items)
            {
                int i = 0;
                foreach (var (label, url) in items)
                    links.Add(new FooterLink { GroupHeading = nhom, Label = label, Url = url, SortOrder = i++ });
            }
            Add("CÔNG TY", ("Giới thiệu", "/about"), ("Tính năng", "/about"), ("Sản phẩm", "/products"), ("Tuyển dụng", "/contact"));
            Add("HỖ TRỢ", ("Hỗ trợ khách hàng", "/contact"), ("Thông tin giao hàng", "/checkout"), ("Điều khoản", "/about"), ("Chính sách bảo mật", "/about"));
            Add("CÂU HỎI THƯỜNG GẶP", ("Tài khoản", "/account"), ("Quản lý giao hàng", "/orders"), ("Đơn hàng", "/orders"), ("Thanh toán", "/checkout"));
            Add("TÀI NGUYÊN", ("Tin tức", "/blog"), ("Hướng dẫn", "/blog"), ("Blog", "/blog"), ("Sản phẩm yêu thích", "/wishlist"));
            context.FooterLinks.AddRange(links);
            context.SaveChanges();
        }

        if (!context.SiteSettings.Any())
        {
            context.SiteSettings.AddRange(
                new SiteSetting { Key = "footer.description", Value = "Chúng tôi có những trang phục hợp phong cách của bạn và khiến bạn tự hào khi khoác lên mình. Từ nữ đến nam." },
                new SiteSetting { Key = "footer.copyright",   Value = "SHOP.CO Create by Quang Van Truong." },
                new SiteSetting { Key = "social.facebook",    Value = "https://www.facebook.com/trugg2" },
                new SiteSetting { Key = "social.messenger",   Value = "https://m.me" },
                new SiteSetting { Key = "social.zalo",        Value = "https://zalo.me" }
            );
            context.SaveChanges();
        }
    }
}
