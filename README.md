# TruongCMS — Đồ án CMS Full-Stack ASP.NET Core

**Họ tên:** Quang Văn Trường | **MSV:** 2123170591  
**Môn học:** Chuyên đề ASP.NET Core | **Giảng viên:** Nguyễn Cao Thái  
**Stack:** ASP.NET Core MVC (.NET 10) + Entity Framework Core + SQL Server + ReactJS

---

## Cấu trúc Solution (3 lớp)

```
TruongCMS_Solution/
├── CMS.Data/                        # Lớp dữ liệu — Entities, DbContext, Migrations
│   ├── Entities/
│   │   ├── Category.cs              # Danh mục tin tức
│   │   ├── Post.cs                  # Bài viết
│   │   ├── User.cs                  # Người dùng quản trị
│   │   ├── CategoryProduct.cs
│   │   ├── Product.cs
│   │   ├── Customer.cs
│   │   ├── Order.cs
│   │   └── OrderDetail.cs
│   ├── ApplicationDbContext.cs
│   ├── DbInitializer.cs             # Seed dữ liệu mẫu lần đầu
│   └── Migrations/
│
├── CMS.Backend/                     # Lớp xử lý — Controllers + Views (MVC)
│   ├── Controllers/
│   │   ├── AdminController.cs       # [MỚI B4] Bảng điều khiển thống kê
│   │   ├── HomeController.cs        # Trang chủ — 3 bài mới nhất
│   │   ├── CategoryController.cs    # CRUD danh mục
│   │   ├── PostController.cs        # CRUD bài viết + upload ảnh
│   │   └── UserController.cs        # CRUD thành viên
│   ├── Views/
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml       # Layout trang công khai
│   │   │   └── _LayoutAdmin.cshtml  # [MỚI B4] Layout khu quản trị (dark sidebar)
│   │   ├── Admin/
│   │   │   └── Index.cshtml         # [MỚI B4] Bảng điều khiển
│   │   ├── Home/Index.cshtml        # Trang chủ công khai
│   │   ├── Category/                # Index, Create, Edit
│   │   ├── Post/                    # Index, Create, Edit, Details
│   │   └── User/                    # [CẬP NHẬT B4] Index, Create, Edit
│   ├── wwwroot/
│   │   ├── images/
│   │   │   ├── no-image.svg         # Ảnh thay thế khi lỗi
│   │   │   └── posts/               # [MỚI B4] img1.png — img6.png
│   │   └── uploads/                 # Ảnh do admin upload (không commit)
│   └── Program.cs
│
├── cms.frontend/                    # Lớp giao diện — ReactJS (buổi 7–9)
└── script/
    └── 01_CreateTables.sql          # Script tạo bảng thủ công
```

---

## Cài đặt & Chạy dự án

### Yêu cầu
- Visual Studio 2022 or 2026(workload: **ASP.NET and web development**)
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

**Cách A — EF Core Migration**:
```bash
# Trong Package Manager Console (chọn Default project: CMS.Data)
Add-Migration InitialCreate
Update-Database
```

**Cách B — Script SQL thủ công**:  
Mở SSMS ->tạo database `TruongCMS_DB` -> chạy `script/01_CreateTables.sql`.

### Bước 3 — Chạy ứng dụng

```bash
dotnet run --project TruongCMS_Solution/CMS.Backend/CMS.Backend.csproj
```

Hoặc nhấn **F5** trong Visual Studio. Ứng dụng tự seed dữ liệu mẫu lần đầu qua `DbInitializer`.

---

## Các trang chức năng

### Trang công khai

| URL | Chức năng |
|-----|-----------|
| `/` | Trang chủ — hiển thị 3 bài viết mới nhất |
| `/Post/Details/{id}` | Chi tiết một bài viết |

### Khu quản trị Admin (`_LayoutAdmin` — dark sidebar)

| URL | Chức năng |
|-----|-----------|
| `/Admin` | **Bảng điều khiển** — thống kê bài viết, danh mục, thành viên |
| `/Category` | Danh sách danh mục |
| `/Category/Create` | Thêm danh mục mới |
| `/Category/Edit/{id}` | Sửa danh mục |
| `/Post` | Danh sách bài viết |
| `/Post/Create` | Thêm bài viết + upload ảnh |
| `/Post/Edit/{id}` | Sửa bài viết |
| `/Post/Details/{id}` | Xem chi tiết bài viết |
| `/User` | Danh sách thành viên |
| `/User/Create` | Thêm thành viên mới |
| `/User/Edit/{id}` | Sửa thông tin thành viên |

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

**Xử lý ảnh:**
- Thêm fallback `no-image.svg` khi ảnh lỗi hoặc không có
- Dùng `onerror="this.src='/images/no-image.svg'"` trên tất cả thẻ `<img>`

**Kiến thức:**  
Cú pháp LINQ lambda, Eager Loading với Include, quy trình 2 bước EF (Add/Update/Remove → SaveChanges), Tag Helper ASP.NET Core.

---

### Buổi 4 — Xây dựng Giao diện Quản trị (Admin Panel) Toàn diện

**Mục tiêu:** Xây dựng khu quản trị hoàn chỉnh với layout riêng, CRUD đầy đủ và upload ảnh.

**Đã thực hiện:**

**Layout Admin (`_LayoutAdmin.cshtml`):**
- Sidebar tối màu Bootstrap (`bg-dark`) có menu điều hướng đến các khu vực
- Bootstrap Icons cho từng mục menu
- `@RenderBody()` — vùng nội dung chính (chỉ gọi đúng 1 lần)
- Áp dụng vào tất cả views admin bằng `@{ Layout = "_LayoutAdmin"; }`

**Bảng điều khiển (`AdminController` + `Views/Admin/Index.cshtml`):**
- 3 thẻ thống kê: Tổng bài viết (xanh) / Tổng danh mục (xanh lá) / Tổng thành viên (xám)
- Bảng 5 bài viết mới nhất kèm danh mục và nút xem nhanh
- Sử dụng `CountAsync()` và `Include().OrderByDescending().Take(5)`

**Post CRUD đầy đủ (`PostController.cs`):**
- `IWebHostEnvironment` inject để lấy đường dẫn `wwwroot`
- `UploadImageAsync(IFormFile?)` — lưu ảnh vào `wwwroot/uploads/` với tên `Guid.NewGuid()`
- `LoadCategoryList()` — helper đổ `SelectList` vào `ViewBag` cho dropdown
- Create: `enctype="multipart/form-data"`, xem trước ảnh bằng `FileReader` JS
- Edit: `AsNoTracking()` để giữ ảnh cũ khi không upload ảnh mới
- Delete: tự động xóa file ảnh local nếu bắt đầu bằng `/uploads/`

**User CRUD đầy đủ (`UserController.cs`):**
- Create: kiểm tra username trùng bằng `AnyAsync()`, báo lỗi qua `ModelState`
- Edit: giữ `Id` bằng `<input type="hidden" asp-for="Id" />`
- Dropdown Role: Admin / Editor / Moderator / User
- Badge màu phân biệt theo quyền: Admin  / Editor  / Moderator / User ⚫

**Ảnh bài viết:**
- 6 ảnh mẫu lưu tại `wwwroot/images/posts/img1.png — img6.png`
- `DbInitializer` tự cập nhật ảnh cũ (URL ngoài) sang ảnh local khi app khởi động

**Kiến thức:**  
Layout override, `@RenderBody()` chỉ gọi 1 lần, `IFormFile` upload, `Guid.NewGuid()` tránh trùng tên file, `AsNoTracking()`, `ModelState.AddModelError()`, `SelectList` + `ViewBag`.

---

## Sơ đồ quan hệ (ERD tóm tắt)

```
Categories ──< Posts
Categories (Products) ──< Products ──< OrderDetails >── Orders >── Customers
Users (quản trị độc lập)
```

---

## Dữ liệu mẫu (seed tự động)

| Bảng | Số dòng mẫu | Ghi chú |
|------|-------------|---------|
| Categories | 5 | Công nghệ, Du lịch, Thể thao, Giáo dục, Lập trình |
| Posts | 5 | Mỗi bài thuộc 1 danh mục, ảnh local `img1–img5` |
| Users | 5 | Admin, Editor, Moderator, User×2 |

> **Lưu ý bảo mật:** Mật khẩu trong `DbInitializer` lưu thô (plain text) chỉ để học tập.  
> Buổi 5 sẽ thay bằng hashing với BCrypt hoặc ASP.NET Core Identity.
