// File khởi động ứng dụng ASP.NET Core (thay cho Startup.cs ở .NET cũ)
// Buổi 2: đăng ký EF Core, chạy Migration, nạp dữ liệu mẫu

using CMS.Data;
using Microsoft.EntityFrameworkCore;

// Tạo builder — đọc appsettings.json, đăng ký dịch vụ
var builder = WebApplication.CreateBuilder(args);

// --- ĐĂNG KÝ DỊCH VỤ (Dependency Injection) ---

// Đăng ký DbContext: mỗi request HTTP sẽ có 1 instance ApplicationDbContext
// Connection string lấy từ appsettings.json → "DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký MVC (Controller + View)
builder.Services.AddControllersWithViews();

// Build app — sau bước này không thêm service được nữa
var app = builder.Build();

// --- KHỞI TẠO DATABASE KHI APP CHẠY ---
using (var scope = app.Services.CreateScope())
{
    // Lấy DbContext từ DI container trong scope tạm
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Migrate: áp dụng migration → tạo/cập nhật bảng trong TruongCMS_DB
    db.Database.Migrate();

    // Seed: nạp dữ liệu mẫu nếu bảng Categories còn trống
    DbInitializer.Seed(db);
}

// --- PIPELINE XỬ LÝ HTTP REQUEST ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Production: bắt lỗi → trang Error
    app.UseHsts();                          // Bảo mật HTTPS
}

app.UseHttpsRedirection();  // Chuyển HTTP → HTTPS
app.UseRouting();           // Phân tích URL → chọn Controller/Action
app.UseAuthorization();     // Kiểm tra quyền (Buổi 5 sẽ dùng nhiều hơn)
app.MapStaticAssets();      // File tĩnh: css, js, ảnh trong wwwroot

// Route mặc định: {controller}/{action}/{id?}
// Ví dụ: /Post/Details/1 → PostController.Details(1)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run(); // Bắt đầu lắng nghe request (F5)
