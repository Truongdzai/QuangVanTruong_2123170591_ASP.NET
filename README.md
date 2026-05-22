# TruongCMS — Đồ án CMS Full-Stack ASP.NET Core

**Họ tên:** Quang Văn Trường | **MSV:** 2123170591  
**Môn học:** Chuyên đề ASP.NET Core  
**Stack:** ASP.NET Core MVC (.NET 10) + Entity Framework Core + SQL Server + ReactJS *(buổi 7–9)*

---

## Cấu trúc Solution (3 lớp)

```
TruongCMS_Solution/
├── CMS.Data/               # Lớp dữ liệu — Entities, DbContext, Migrations
│   ├── Entities/
│   │   ├── Category.cs     # Danh mục tin tức
│   │   ├── Post.cs         # Bài viết
│   │   ├── User.cs         # Người dùng quản trị
│   │   ├── CategoryProduct.cs
│   │   ├── Product.cs
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   └── OrderDetail.cs
│   ├── ApplicationDbContext.cs
│   ├── DbInitializer.cs    # Seed dữ liệu mẫu lần đầu
│   └── Migrations/
│
├── CMS.Backend/            # Lớp xử lý — Controllers + Views (MVC)
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── CategoryController.cs
│   │   ├── PostController.cs
│   │   └── UserController.cs
│   ├── Views/
│   │   ├── Home/Index.cshtml
│   │   ├── Category/Index.cshtml
│   │   ├── Category/Create.cshtml
│   │   ├── Category/Edit.cshtml
│   │   ├── Post/Index.cshtml
│   │   ├── Post/Details.cshtml
│   │   └── User/Index.cshtml
│   └── Program.cs
│
├── cms.frontend/           # Lớp giao diện — ReactJS (buổi 7–9)
└── script/
    └── 01_CreateTables.sql # Script tạo 8 bảng thủ công (thay cho Migration)
```

---

## Cài đặt & Chạy dự án

### Yêu cầu
- Visual Studio 2022 (workload: **ASP.NET and web development**)
- .NET 10 SDK
- SQL Server Express hoặc LocalDB

### Bước 1 — Cấu hình Connection String

Mở `CMS.Backend/appsettings.json` và chỉnh `ConnectionStrings`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TruongCMS_DB;Trusted_Connection=True;"
}
```

### Bước 2 — Tạo Database

**Cách A — EF Core Migration** (khuyến nghị):
```bash
# Trong Package Manager Console (chọn Default project: CMS.Data)
Add-Migration InitialCreate
Update-Database
```

**Cách B — Script SQL thủ công**:  
Mở SSMS → tạo database `TruongCMS_DB` → chạy `script/01_CreateTables.sql`.

### Bước 3 — Chạy ứng dụng

```bash
dotnet run --project TruongCMS_Solution/CMS.Backend/CMS.Backend.csproj
```

Hoặc nhấn **F5** trong Visual Studio. Ứng dụng tự seed dữ liệu mẫu lần đầu qua `DbInitializer`.

---

## Các trang chức năng

| URL | Chức năng |
|-----|-----------|
| `/` | Trang chủ — hiển thị 3 bài viết mới nhất |
| `/Category` | Danh sách danh mục (có nút Thêm/Sửa/Xóa) |
| `/Category/Create` | Form thêm danh mục mới |
| `/Category/Edit/{id}` | Form chỉnh sửa danh mục |
| `/Post` | Danh sách toàn bộ bài viết |
| `/Post/Index/{id}` | Lọc bài viết theo danh mục |
| `/Post/Details/{id}` | Chi tiết một bài viết |
| `/User` | Danh sách tài khoản hệ thống |

---

## Tiến độ theo buổi học

### Buổi 1 — Khởi tạo cấu trúc Solution

**Mục tiêu:** Tạo Solution 3 lớp, định nghĩa 8 Entity.

**Đã thực hiện:**
- Tạo Blank Solution `TruongCMS_Solution`
- Tạo project `CMS.Data` (Class Library) với 8 Entity: `Category`, `Post`, `User`, `CategoryProduct`, `Product`, `Customer`, `Order`, `OrderDetail`
- Tạo project `CMS.Backend` (ASP.NET Core Web App MVC)
- Tạo `CategoryController`, `PostController`, `UserController` với dữ liệu giả (mock)
- Tạo Views cơ bản cho Category, Post, User

**Kiến thức:**  
Cấu trúc Solution 3 lớp, Dependency Injection cơ bản, MVC pattern.

---

### Buổi 2 — Entity Framework Core & Kết nối SQL Server

**Mục tiêu:** Thay dữ liệu giả bằng dữ liệu thật từ SQL Server.

**Đã thực hiện:**
- Cài đặt NuGet: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`
- Tạo `ApplicationDbContext` với `DbSet` cho 8 bảng
- Cấu hình `Connection String` trong `appsettings.json`
- Đăng ký `DbContext` trong `Program.cs` qua Dependency Injection
- Chạy `Add-Migration InitialCreate` + `Update-Database` → tạo `TruongCMS_DB`
- Cập nhật 3 Controller dùng `_context` thay cho dữ liệu giả
- Tạo `DbInitializer.Seed()` để nạp dữ liệu mẫu tự động

**Kiến thức:**  
DbContext, Connection String, Migration workflow, `async/await` với `ToListAsync()`.

---

### Buổi 3 — Truy vấn LINQ & Thao tác dữ liệu chuyên sâu

**Mục tiêu:** Thành thạo LINQ và hoàn thiện CRUD cho Category.

**Đã thực hiện:**

**LINQ nâng cao:**
- `Where(p => p.CategoryId == id)` — lọc bài viết theo danh mục
- `OrderByDescending(p => p.CreatedDate)` — sắp xếp mới nhất lên đầu
- `.Include(p => p.Category)` — Eager Loading, tránh lỗi `null` khi truy xuất navigation property
- `.Take(3)` — lấy đúng N bản ghi đầu tiên

**CRUD cho Category** (`CategoryController.cs`):
- `GET /Category/Create` → hiển thị form thêm mới
- `POST /Category/Create` → `Add()` + `SaveChanges()` → lưu vào SQL
- `GET /Category/Edit/{id}` → tải dữ liệu cũ lên form
- `POST /Category/Edit` → `Update()` + `SaveChanges()` → cập nhật SQL
- `GET /Category/Delete/{id}` → `Remove()` + `SaveChanges()` → xóa SQL

**Cập nhật Controller:**
- `PostController.Index(int? id)` — hỗ trợ lọc theo CategoryId + Include
- `PostController.Details(int id)` — Include Category để hiển thị tên danh mục
- `HomeController.Index()` — Inject DbContext, LINQ lấy 3 bài mới nhất

**Views mới / cập nhật:**
- `Category/Create.cshtml` — form thêm mới với Tag Helper `asp-for`, `asp-action`
- `Category/Edit.cshtml` — form sửa với `<input type="hidden" asp-for="Id" />`
- `Category/Index.cshtml` — thêm nút Sửa/Xóa, nút Thêm danh mục mới
- `Post/Index.cshtml` — thêm badge tên danh mục
- `Post/Details.cshtml` — hiển thị tên danh mục
- `Home/Index.cshtml` — hiển thị 3 card bài viết mới nhất

**Kiến thức:**  
Cú pháp LINQ lambda, Eager Loading với Include, quy trình 2 bước EF (Add/Update/Remove → SaveChanges), Tag Helper ASP.NET Core (`asp-for`, `asp-action`, `asp-route-id`).

---

## Sơ đồ quan hệ (ERD tóm tắt)

```
Categories ──< Posts
Categories (Products) ──< Products ──< OrderDetails >── Orders >── Customers
Users (quản trị độc lập)
```

---

## Dữ liệu mẫu (seed tự động)

| Bảng | Số dòng mẫu |
|------|-------------|
| Categories | 5 danh mục tin tức |
| Posts | 5 bài viết (mỗi bài thuộc 1 danh mục) |
| Users | 5 tài khoản (Admin, Editor, Moderator, User×2) |

> **Lưu ý bảo mật:** Mật khẩu trong `DbInitializer` lưu thô (plain text) chỉ để học tập.  
> Buổi 5 sẽ thay bằng hashing với ASP.NET Core Identity.
