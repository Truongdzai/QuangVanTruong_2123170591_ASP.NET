// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: 5
// Ngay thuc hien: 26/05/2026
// Version: 1.5

using CMS.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dang ky DbContext: moi request se co 1 instance ApplicationDbContext
// Connection string lay tu appsettings.json -> "DefaultConnection"
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dang ky MVC (Controller + Razor View)
builder.Services.AddControllersWithViews();

// Buoi 5: Khai bao dich vu xac thuc Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";           // Chua dang nhap -> ve trang nay
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Buoi 5: Kiem tra "Anh la ai?" (doc Cookie)
app.UseAuthorization();  // Buoi 5: Kiem tra "Anh duoc lam gi?" (kiem tra quyen)

// Cho phep phuc vu file tinh (css, js, anh, uploads) tu wwwroot
app.MapStaticAssets();

// Route mac dinh: /Controller/Action/{id?}
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
