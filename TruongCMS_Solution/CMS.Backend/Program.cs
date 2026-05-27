// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 6
// Ngay thuc hien: 27/05/2026
// Version: 1.6

using CMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. KHU VUC DANG KY DICH VU (SERVICES CONTAINER)

// Dang ky DbContext: moi request se co 1 instance ApplicationDbContext
// Connection string lay tu appsettings.json -> "DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dang ky MVC vua phuc vu Web MVC (.cshtml) vua nhan dien API Controller
builder.Services.AddControllersWithViews();

// Buoi 6: Dang ky dich vu lo OpenAPI de Swagger tu dong sinh tai lieu API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Buoi 6: Chinh sach CORS - mo cong hop phap cho ReactJS (cong khac) ket noi
// Luu y: chi dung AllowAnyOrigin() trong moi truong hoc. Thuc te phai chi ro URL cu the.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Buoi 5: Khai bao dich vu xac thuc Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";            // Chua dang nhap -> ve trang nay
        options.AccessDeniedPath = "/Account/AccessDenied"; // Khong du quyen -> ve trang nay
    });

var app = builder.Build();

// Khoi tao database khi app chay lan dau
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Migrate: tao/cap nhat bang trong SQL Server theo Migration
    db.Database.Migrate();

    // Seed: them du lieu mau neu chua co
    DbInitializer.Seed(db);
}

// 2. KHU VUC CAU HINH MIDDLEWARE (REQUEST PIPELINE)
// Thu tu cac middleware rat quan trong, khong duoc doi cho tuy tien

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Buoc 1: Xac dinh duong dan (phai co truoc CORS, Swagger, Auth)
app.UseRouting();

// Buoc 2: CORS - phai sau UseRouting, truoc UseAuthentication
app.UseCors("AllowAll");

// Buoc 3: Buoi 6 - Kich hoat Swagger middleware (phai sau UseRouting)
// Truy cap tai: https://localhost:xxxx/swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TruongCMS Web API v1");
    c.RoutePrefix = "swagger";
});

// Buoc 4: Buoi 5 - Xac thuc & Phan quyen
app.UseAuthentication(); // Kiem tra "Anh la ai?" (doc Cookie)
app.UseAuthorization();  // Kiem tra "Anh duoc lam gi?" (kiem tra quyen)

// Cho phep phuc vu file tinh (css, js, anh, uploads) tu wwwroot
app.MapStaticAssets();

// 3. KHU VUC DINH TUYEN PHAN LUONG (ROUTING MAP)

// Phan luong A: API endpoints - theo [Route("api/[controller]")]
// Vi du: /api/posts, /api/CategoriesProducts
app.MapControllers();

// Phan luong B: Web MVC cu - giu nguyen route mac dinh
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
