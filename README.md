# Buổi 2 - Entity Framework Core & Kết nối SQL Server

| Thông tin | Chi tiết |
|-----------|----------|
| Sinh viên | Quang Văn Trường |
| MSV | 2123170591 |
| Solution | TruongCMS_Solution |
| Database | TruongCMS_DB |
| Tiên quyết | Hoàn thành Buổi 1 |

---

## 1. Mục tiêu buổi học

Sau Buổi 2, bạn có thể:

- Cài đặt EF Core và kết nối SQL Server / LocalDB.
- Dùng Code First Migration: class C# -> tạo bảng SQL tự động.
- Hiểu Dependency Injection (DI) - inject ApplicationDbContext vào Controller.
- Thay mock data (Buổi 1) bằng dữ liệu thật từ database.
- Kiểm tra database bằng SSMS hoặc script SQL trong folder script/.

---

## 2. So sánh Buổi 1 vs Buổi 2

| Hạng mục | Buổi 1 | Buổi 2 |
|----------|--------|--------|
| Nguồn dữ liệu | new List trong Controller | _context.Categories.ToListAsync() |
| Lưu trữ | RAM khi app chạy | SQL Server - TruongCMS_DB |
| CMS.Data | Chỉ Entity | + ApplicationDbContext, Migrations |
| Program.cs | AddControllersWithViews | + AddDbContext, Migrate, Seed |

Luồng Buổi 2:

```
Controller -> _context -> EF Core -> SQL Server -> View
```

---

## 3. Công nghệ & package NuGet

### CMS.Data (CMS.Data.csproj)

```
Microsoft.EntityFrameworkCore.SqlServer   (10.0.0)
Microsoft.EntityFrameworkCore.Design      (10.0.0)
```

### CMS.Backend (CMS.Backend.csproj)

```
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.Tools       (lệnh dotnet ef)
```

Lưu ý: chọn version EF trùng với .NET 10 của project.

---

## 4. Các file quan trọng Buổi 2

| File | Chức năng |
|------|-----------|
| CMS.Data/ApplicationDbContext.cs | Khai báo DbSet cho 8 bảng |
| CMS.Data/DbInitializer.cs | Nạp dữ liệu mẫu lần đầu |
| CMS.Data/Migrations/ | Lịch sử thay đổi schema |
| CMS.Backend/appsettings.json | Connection string |
| CMS.Backend/Program.cs | Đăng ký DbContext, Migrate, Seed |
| script/*.sql | Tạo DB thủ công trong SSMS |

---

## 5. ApplicationDbContext

Mỗi DbSet = 1 bảng trong SQL:

```
DbSet<Category>           -> bảng Categories
DbSet<Post>               -> bảng Posts
DbSet<User>               -> bảng Users
DbSet<CategoryProduct>    -> bảng CategoriesProducts
DbSet<Product>            -> bảng Products
DbSet<Customer>           -> bảng Customers
DbSet<Order>              -> bảng Orders
DbSet<OrderDetail>        -> bảng OrderDetails
```

OnModelCreating: cấu hình khóa ngoại, quy tắc xóa (Restrict / Cascade).

---

## 6. Connection String

File: CMS.Backend/appsettings.json

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TruongCMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

| Môi trường | Server gợi ý |
|------------|--------------|
| LocalDB | (localdb)\mssqllocaldb |
| SQL Express | .\SQLEXPRESS |
| Instance mặc định | . |

---

## 7. Đăng ký DbContext (Program.cs)

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Dependency Injection:

```
HTTP Request -> CategoryController
              -> framework tự tạo ApplicationDbContext
              -> truyền vào constructor
```

Khởi tạo DB khi app chạy:

```csharp
db.Database.Migrate();      // áp dụng migration
DbInitializer.Seed(db);     // nạp dữ liệu mẫu nếu DB trống
```

---

## 8. Migration - Tạo database

### Cài công cụ (một lần)

```powershell
dotnet tool install --global dotnet-ef
```

### Tạo migration (lần đầu - đã có trong repo thì bỏ qua)

```powershell
cd d:\asp.NET\TruongCMS_Solution\CMS.Backend

dotnet ef migrations add InitialCreate --project ..\CMS.Data\CMS.Data.csproj
```

### Áp dụng lên SQL Server (tạo DB + bảng)

```powershell
dotnet ef database update --project ..\CMS.Data\CMS.Data.csproj
```

### Package Manager Console (Visual Studio)

```powershell
# Default project: CMS.Data
Add-Migration InitialCreate
Update-Database
```

---

## 9. Tạo DB bằng SSMS (script SQL)

Folder: TruongCMS_Solution/script/

| File | Mô tả |
|------|--------|
| 00_CreateDatabase.sql | Tạo TruongCMS_DB |
| 01_CreateTables.sql | 8 bảng + FK |
| 02_SeedData.sql | Dữ liệu mẫu |
| 03_Query_Check.sql | Truy vấn kiểm tra |
| 99_DropDatabase.sql | Xóa DB |
| Chạy 00 -> 01 -> 02 |

Cách dùng SSMS:

1. Connect: (localdb)\mssqllocaldb
2. Mở chạy lần lượt 00, 01, 02
3. F5 Execute

---

## 10. Controller sau Buổi 2

### CategoryController

```csharp
public async Task<IActionResult> Index()
{
    var data = await _context.Categories.ToListAsync();
    return View(data);
}
```

### PostController

- Index: OrderByDescending(p => p.CreatedDate) -> bài mới lên đầu
- Details: FirstOrDefaultAsync(p => p.Id == id)
  - post == null -> return NotFound() (HTTP 404)
  - có dữ liệu -> return View(post)

### UserController

- Index: _context.Users.ToListAsync()
- View không hiển thị PasswordHash

---

## 11. Dữ liệu mẫu (Seed)

Nguồn: DbInitializer.cs hoặc script/02_SeedData.sql

### Categories (5 dòng)

- Tin tức Công nghệ
- Đời sống du lịch
- Sức khỏe Thể thao
- Giáo dục Kỹ năng
- Góc lập trình viên

### Posts (5 bài)

- CategoryId phải tồn tại trong bảng Categories (1 <= CategoryId <= 5)

### Users (5 tài khoản)

| Username | Role |
|----------|------|
| admin | Admin |
| thai_gv | Editor |
| sv_01 | User |
| sv_02 | User |
| moderator | Moderator |

---

## 12. 8 bảng trong TruongCMS_DB

| Bảng | Entity |
|------|--------|
| Categories | Category |
| Posts | Post |
| Users | User |
| CategoriesProducts | CategoryProduct |
| Products | Product |
| Customers | Customer |
| Orders | Order |
| OrderDetails | OrderDetail |

Quan hệ chính:

```
Category         1 -> n  Post
CategoryProduct  1 -> n  Product
Customer         1 -> n  Order
Order            1 -> n  OrderDetail
OrderDetail -> Product (khóa ngoại ProductId)
```

---

## 13. Chạy thử & Checkpoint Buổi 2

### Chạy

```powershell
cd d:\asp.NET\TruongCMS_Solution\CMS.Backend
dotnet build
dotnet run
```

Hoặc F5 trong Visual Studio (Startup: CMS.Backend).

### Checkpoint

| # | Việc cần làm | Đạt |
|---|--------------|-----|
| 1 | SSMS thấy database TruongCMS_DB | [ ] |
| 2 | Có đủ 8 bảng | [ ] |
| 3 | /Category hiện 5 danh mục từ DB | [ ] |
| 4 | /Post hiện 5 bài viết | [ ] |
| 5 | /User hiện 5 user (không có password) | [ ] |
| 6 | /Post/Details/1 hiện chi tiết | [ ] |
| 7 | /Post/Details/999 -> 404 | [ ] |

### Bài tập rèn luyện

1. Post - đảm bảo CategoryId hợp lệ trong SSMS.
2. User - nhập thêm user trong SSMS, F5 xem trên web.
3. (Tùy chọn) Tạo Controller + View cho CategoriesProducts, Products.

---

## 14. Lỗi thường gặp & xử lý

| Lỗi | Nguyên nhân | Cách xử lý |
|-----|-------------|------------|
| Build failed khi dotnet ef | App đang chạy (F5) khóa .dll | Stop debug hoặc taskkill CMS.Backend.exe |
| Cannot open database | Sai connection string / SQL chưa chạy | Kiểm tra SSMS; sửa appsettings.json |
| Migration đã tồn tại | Chạy lại add InitialCreate | Chỉ chạy database update |
| Trang trống, không lỗi | DB chưa seed | Xóa DB -> update lại hoặc chạy 02_SeedData.sql |
| NU1903 warning | Package transitive | Cảnh báo, không chặn build |

### Kiểm tra SQL Server trước Migration

1. services.msc -> SQL Server -> Running
2. SSMS -> Connect (localdb)\mssqllocaldb
3. Query: SELECT name FROM sys.databases

---

## 15. Sơ đồ luồng Buổi 2

```
appsettings.json (Connection String)
    -> Program.cs -> AddDbContext<ApplicationDbContext>
    -> CategoryController -> _context.Categories.ToListAsync()
    -> EF Core -> SQL: SELECT * FROM Categories
    -> SQL Server (TruongCMS_DB)
    -> View Index.cshtml -> hiện bảng HTML
```

---

## 16. Chuẩn bị Buổi 3

Buổi 3: LINQ nâng cao + CRUD (Thêm / Sửa / Xóa) cho Category, Post.

- Where, Include, OrderBy
- Form GET/POST, SaveChanges()

---

## 17. Tài liệu tham khảo

- README-Buoi-1.md
- Giáo trình: chuyen-de-ASP.NET.docx - Buổi 2
- https://learn.microsoft.com/ef/core/managing-schemas/migrations/
- https://learn.microsoft.com/ef/core/dbcontext-configuration/
- Script SQL: ../script/
