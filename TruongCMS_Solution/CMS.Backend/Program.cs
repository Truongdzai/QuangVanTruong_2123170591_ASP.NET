// Ho ten: Quang Van Truong || MSV: 2123170591
// Mon hoc: ASP.NET || Giang vien: Nguyen Cao Thai
// Bai thuc hanh: Nang cap he thong theo BAO CAO NGHIEN CUU CHUYEN SAU
// Ngay thuc hien: 12/06/2026
// Version: 3.0
//
// Cac muc bao cao duoc noi day o file nay:
//  - Muc 7 Serilog      : log co cau truc ra console + file JSON (logs/)
//  - Muc 7 Hangfire     : tac vu nen (huy don qua han, email, don dep ma sale)
//  - Muc 7 Caching      : IDistributedCache — RAM mac dinh, Redis khi co cau hinh
//  - Muc 4 IAM          : Cookie (Admin MVC) + JWT Bearer (khach React) song song;
//                         API tra 401/403 JSON thay vi redirect (test case 14)
//  - Bao mat            : Rate limiting chong brute-force dang nhap (OWASP)

using CMS.Data;
using CMS.Backend.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ───────────────────────── SERILOG (muc 7 — Logging) ─────────────────────────
// Log co cau truc: console de dev nhin nhanh, file JSON (CompactJsonFormatter)
// de day len he thong tap trung (Seq/ELK) sau nay. Giu 14 ngay gan nhat.
builder.Host.UseSerilog((ctx, cfg) => cfg
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Hangfire", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new CompactJsonFormatter(),
        path: "logs/log-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14));

// 1. KHU VUC DANG KY DICH VU (SERVICES CONTAINER)

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// Swagger sinh tai lieu API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────── CORS cho ung dung ReactJS (muc bao mat) ────────────────────
// Ung dung React (Vite) chay o PORT 3000 — xem vite.config.ts (server.port: 3000).
// Origin chinh lay tu cau hinh "FrontendUrl" de chi khai bao MOT noi (cung gia tri
// dung cho redirect sau thanh toan VNPAY), kem port 4173 cho ban build `vite preview`.
// Khong dung AllowAnyOrigin: chi cho dung CHINH XAC cac origin React duoc tin cay.
var reactAppOrigins = new[]
{
    builder.Configuration["FrontendUrl"] ?? "http://localhost:3000", // Vite dev — port 3000
    "http://localhost:4173",                                         // Vite preview (build thu local)
}
.Where(o => !string.IsNullOrWhiteSpace(o))
.Select(o => o.TrimEnd('/'))
.Distinct()
.ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(reactAppOrigins)   // chi mo cho cong React chinh xac (3000)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─────────────── CACHING (muc 7 — Redis/Memory qua IDistributedCache) ───────
// Co "Redis:ConnectionString" trong appsettings -> dung Redis that;
// chua co -> cache RAM. Controller chi biet ICacheService, khong phai sua gi.
string? redisConn = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(o =>
    {
        o.Configuration = redisConn;
        o.InstanceName = "TruongCMS:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// ─────────────── HANGFIRE (muc 7 — Background Jobs) ─────────────────────────
// Overload nhan IServiceProvider -> storage khoi tao TRE (sau khi DB da Migrate),
// tranh loi lan chay dau tien khi database chua ton tai.
builder.Services.AddHangfire((sp, cfg) => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true,
            QueuePollInterval = TimeSpan.FromSeconds(15)
        }));
builder.Services.AddHangfireServer();

// ─────────────── DANG KY CAC SERVICE NGHIEP VU ───────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CMS.Backend.Services.IEmailService, CMS.Backend.Services.SmtpEmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();              // JWT + refresh (test case 15)
builder.Services.AddScoped<IPricingService, PricingService>();          // gia flash sale theo gio server (test case 9)
builder.Services.AddScoped<IVnPayService, VnPayService>();              // cong thanh toan + HMAC (test case 10-11)
builder.Services.AddScoped<IShippingService, MockGhtkShippingService>();// van chuyen GHTK mock (muc 2)
builder.Services.AddScoped<IOrderWorkflowService, OrderWorkflowService>(); // may trang thai don (test case 12)
builder.Services.AddScoped<ICacheService, CacheService>();              // cache-aside (muc 7)
builder.Services.AddScoped<IFileUploadService, FileUploadService>();    // upload anh dung chung
builder.Services.AddScoped<BackgroundJobs>();                           // cac job Hangfire

// ─────────────── XAC THUC 2 LOP: COOKIE (Admin MVC) + JWT (React) ────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // TEST CASE 14: duong dan /api/* khong duoc redirect ve trang Login HTML —
        // phai tra 401 (chua dang nhap) / 403 (thieu quyen) dang JSON cho Frontend xu ly
        options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    })
    // JWT Bearer cho khach hang ReactJS (test case 15): access token 15 phut,
    // het han tra 401 -> Frontend tu dong goi /api/customers/refresh-token
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "TruongCMS",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TruongCMS.Frontend",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30), // het han la het han — khong du di 5 phut mac dinh
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.GetSigningKey(builder.Configuration)
        };
    });

// ─────────────── RATE LIMITING chong brute-force (bao cao muc bao mat) ───────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json; charset=utf-8";
        await ctx.HttpContext.Response.WriteAsync(
            "{\"message\":\"Bạn thao tác quá nhanh — vui lòng thử lại sau 1 phút.\"}", ct);
    };

    // Moi IP toi da 10 lan dang nhap/dang ky moi phut (chong do mat khau)
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// Khoi tao database khi app chay lan dau
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Seed(db);
}

// 2. KHU VUC CAU HINH MIDDLEWARE (REQUEST PIPELINE)

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Serilog ghi log 1 dong/request (method, path, status, thoi gian xu ly)
app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors("AllowReactApp");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TruongCMS Web API v1");
    c.RoutePrefix = "swagger";
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapStaticAssets();

// ─────────────── HANGFIRE DASHBOARD (chi Admin xem duoc) ─────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "TruongCMS - Tác vụ nền (Hangfire)",
    Authorization = new[] { new CMS.Backend.Services.HangfireAdminFilter() }
});

// Dang ky RECURRING JOBS (sau khi DB da migrate o tren)
using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // 5 phut/lan: huy don VNPAY qua 15 phut chua thanh toan + HOAN KHO
    recurring.AddOrUpdate<BackgroundJobs>(
        "huy-don-qua-han-thanh-toan",
        j => j.HuyDonQuaHanThanhToan(),
        "*/5 * * * *");

    // 0h05 hang ngay: tat cac ma giam gia het han / het luot
    recurring.AddOrUpdate<BackgroundJobs>(
        "tat-ma-giam-gia-het-han",
        j => j.TatMaGiamGiaHetHan(),
        Cron.Daily(0, 5));
}

// 3. KHU VUC DINH TUYEN PHAN LUONG (ROUTING MAP)

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
