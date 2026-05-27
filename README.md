# TruongCMS —  ASP.NET Core

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
├── CMS.Backend/                     # Lớp xử lý — Controllers + Views (MVC) + Web API
│   ├── Controllers/
│   │   ├── — MVC Controllers (trả về Views) —
│   │   ├── AdminController.cs       # [B4] Bảng điều khiển; [B6] thêm thống kê thương mại
│   │   ├── HomeController.cs        # Trang chủ — 3 bài mới nhất
│   │   ├── AccountController.cs     # [B5] Login / Logout / AccessDenied
│   │   ├── CategoryController.cs    # CRUD danh mục tin tức
│   │   ├── PostController.cs        # CRUD bài viết + upload ảnh
│   │   ├── UserController.cs        # CRUD thành viên quản trị
│   │   ├── CategoryProductController.cs  # [B6] CRUD danh mục sản phẩm
│   │   ├── ProductController.cs     # [B6] CRUD sản phẩm + upload ảnh
│   │   ├── CustomerController.cs    # [B6] Xem & xóa khách hàng
│   │   ├── OrderController.cs       # [B6] Xem, cập nhật trạng thái & xóa đơn hàng
│   │   ├── — API Controllers (trả về JSON, kế thừa ControllerBase) —
│   │   ├── PostsController.cs       # [B6] GET /api/posts
│   │   ├── ProductsController.cs    # [B6] GET /api/products
│   │   ├── CategoriesProductsController.cs  # [B6] GET /api/CategoriesProducts
│   │   └── OrdersController.cs      # [B6] POST /api/Orders
│   ├── Views/
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml       # Layout trang công khai
│   │   │   └── _LayoutAdmin.cshtml  # [B4] Layout khu quản trị (dark sidebar)
│   │   ├── Account/                 # [B5] Login.cshtml, AccessDenied.cshtml
│   │   ├── Admin/
│   │   │   └── Index.cshtml         # [B4+B6] Bảng điều khiển 7 thẻ + 2 bảng
│   │   ├── Home/Index.cshtml        # Trang chủ công khai
│   │   ├── Category/                # Index, Create, Edit
│   │   ├── Post/                    # Index, Create, Edit, Details
│   │   ├── User/                    # Index, Create, Edit
│   │   ├── CategoryProduct/         # [B6] Index, Create, Edit
│   │   ├── Product/                 # [B6] Index, Create, Edit
│   │   ├── Customer/                # [B6] Index, Details
│   │   └── Order/                   # [B6] Index, Details
│   ├── wwwroot/
│   │   ├── images/
│   │   │   ├── no-image.svg         # Ảnh thay thế khi lỗi
│   │   │   └── posts/               # [B4] img1.png — img6.png
│   │   └── uploads/                 # Ảnh do admin upload 
│   └── Program.cs
│
├── cms.frontend/                    # Lớp giao diện — ReactJS 
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

### Xác thực (`[B5]`)

| URL | Chức năng |
|-----|-----------|
| `/Account/Login` | Form đăng nhập |
| `/Account/Logout` | Đăng xuất, xóa Cookie |
| `/Account/AccessDenied` | Trang thông báo không đủ quyền |

### Khu quản trị Admin (`_LayoutAdmin` — dark sidebar)

| URL | Chức năng |
|-----|-----------|
| `/Admin` | **Bảng điều khiển** — 7 thẻ thống kê + 5 bài/đơn mới nhất |
| `/Category` | Danh sách danh mục tin tức |
| `/Category/Create` | Thêm danh mục |
| `/Category/Edit/{id}` | Sửa danh mục |
| `/Post` | Danh sách bài viết |
| `/Post/Create` | Thêm bài viết + upload ảnh |
| `/Post/Edit/{id}` | Sửa bài viết |
| `/Post/Details/{id}` | Xem chi tiết bài viết |
| `/User` | Danh sách thành viên |
| `/User/Create` | Thêm thành viên |
| `/User/Edit/{id}` | Sửa thành viên |
| `/CategoryProduct` | **[B6]** Danh sách danh mục sản phẩm |
| `/CategoryProduct/Create` | **[B6]** Thêm danh mục sản phẩm |
| `/CategoryProduct/Edit/{id}` | **[B6]** Sửa danh mục sản phẩm |
| `/Product` | **[B6]** Danh sách sản phẩm |
| `/Product/Create` | **[B6]** Thêm sản phẩm + upload ảnh |
| `/Product/Edit/{id}` | **[B6]** Sửa sản phẩm |
| `/Customer` | **[B6]** Danh sách khách hàng |
| `/Customer/Details/{id}` | **[B6]** Hồ sơ khách hàng + lịch sử đơn hàng |
| `/Order` | **[B6]** Danh sách đơn hàng |
| `/Order/Details/{id}` | **[B6]** Chi tiết đơn hàng + cập nhật trạng thái |

### Web API REST (`[B6]` — JSON, dùng cho ReactJS)

| Method | URL | Mô tả |
|--------|-----|-------|
| GET | `/api/posts` | Danh sách bài viết (tóm tắt) |
| GET | `/api/posts/{id}` | Chi tiết bài viết |
| GET | `/api/posts/category/{categoryId}` | Lọc bài viết theo danh mục |
| GET | `/api/products` | Danh sách sản phẩm |
| GET | `/api/products/{id}` | Chi tiết sản phẩm |
| GET | `/api/products/categoryproduct/{id}` | Lọc sản phẩm theo danh mục |
| GET | `/api/CategoriesProducts` | Danh sách danh mục sản phẩm |
| GET | `/api/CategoriesProducts/{id}` | Chi tiết danh mục sản phẩm |
| POST | `/api/Orders` | Đặt hàng mới (nhận `OrderInputDTO` JSON) |

Tài liệu API tự động tại: **`/swagger`**

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

### Buổi 5 — Bảo mật & Phân quyền (Security & Identity)

**Mục tiêu:** Bảo vệ khu quản trị bằng Cookie Authentication và phân quyền theo Role.

**Đã thực hiện:**

**Đăng ký dịch vụ xác thực (`Program.cs`):**
- `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)` — khai báo cơ chế xác thực Cookie
- `LoginPath = "/Account/Login"` — tự động chuyển hướng khi chưa đăng nhập
- `AccessDeniedPath = "/Account/AccessDenied"` — tự động chuyển hướng khi không đủ quyền
- `app.UseAuthentication()` → `app.UseAuthorization()` — thứ tự middleware quan trọng

**`AccountController` (mới):**
- `GET /Account/Login` → hiển thị form, nếu đã đăng nhập thì chuyển thẳng vào Admin
- `POST /Account/Login` → tìm user trong DB, tạo `ClaimsPrincipal` (họ tên, username, role), gọi `SignInAsync()` để lưu Cookie
- `GET /Account/Logout` → gọi `SignOutAsync()`, về trang Login
- `GET /Account/AccessDenied` → trang thông báo không đủ quyền

**Claims — "Chứng minh nhân dân số" của người dùng:**
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.Username),
    new Claim(ClaimTypes.Role, user.Role),   // Admin / Editor / Moderator / User
    new Claim("FullName", user.FullName)
};
```

**Bảo vệ các Controller:**
- `[Authorize]` trên class — toàn bộ action yêu cầu đăng nhập
- `[Authorize(Roles = "Admin")]` — chỉ Admin mới được thực hiện (VD: xóa đơn hàng)

**Dữ liệu đăng nhập mẫu:**

| Username | Password | Role |
|----------|----------|------|
| `admin` | `admin123` | Admin |
| `editor` | `editor123` | Editor |

**Kiến thức:**  
Cookie Authentication, Claims-based identity, `ClaimsPrincipal`, `SignInAsync`/`SignOutAsync`, `[Authorize]`, thứ tự middleware `UseAuthentication` -> `UseAuthorization`.

---

### Buổi 6 — Web API, CRUD Thương mại điện tử, Swagger & CORS

**Mục tiêu:** Xây dựng REST API cho ReactJS frontend, hoàn thiện CRUD thương mại điện tử, tích hợp Swagger và cấu hình CORS.

**Đã thực hiện:**

**1. REST API Controllers (`[ApiController]` + `ControllerBase`):**

Khác biệt so với MVC Controller:
| | MVC Controller | API Controller |
|-|---------------|----------------|
| Kế thừa | `Controller` | `ControllerBase` |
| Trả về | View (HTML) | JSON |
| Attribute | (không bắt buộc) | `[ApiController]` |
| Route | `[Route("api/[controller]")]` | `[Route("api/[controller]")]` |

Các API đã xây dựng:
- **`PostsController`**: `GET /api/posts`, `GET /api/posts/{id}`, `GET /api/posts/category/{categoryId}`
- **`ProductsController`**: `GET /api/products`, `GET /api/products/{id}`, `GET /api/products/categoryproduct/{id}`
- **`CategoriesProductsController`**: `GET /api/CategoriesProducts`, `GET /api/CategoriesProducts/{id}`
- **`OrdersController`**: `POST /api/Orders` — nhận `OrderInputDTO` từ body JSON, tạo đơn hàng mới, trả về `201 Created`

Kỹ thuật Projection (got tia) — chỉ trả về các trường cần thiết:
```csharp
.Select(p => new { p.Id, p.Title, p.ImageUrl, CategoryName = p.Category.Name })
```

**2. Swagger (tài liệu API tự động):**
```csharp
// Program.cs — đăng ký
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Program.cs — kích hoạt middleware
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TruongCMS Web API v1");
    c.RoutePrefix = "swagger";
});
```
Truy cập tại: `https://localhost:{port}/swagger`

**3. CORS (Cross-Origin Resource Sharing):**
```csharp
// Đăng ký — mở cổng cho ReactJS (port khác) kết nối
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Middleware — phải đặt sau UseRouting, trước UseAuthentication
app.UseCors("AllowAll");
```

**4. MVC Controllers mới (quản trị thương mại):**

- **`ProductController`**: CRUD sản phẩm + upload ảnh (giống `PostController`)
- **`CategoryProductController`**: CRUD danh mục sản phẩm
- **`CustomerController`**: Index (danh sách + số đơn), Details (`ThenInclude` 3 cấp), Delete (`[Authorize(Roles = "Admin")]`)
- **`OrderController`**: Index, Details, `POST /Order/UpdateStatus` (cập nhật trạng thái 0→1→2), Delete (`Admin only`)

**5. Mở rộng bảng điều khiển Admin:**
- Từ 3 thẻ (B4) → 7 thẻ: Bài viết (xanh), Danh mục (xanh lá), Thành viên (xám), Sản phẩm (tím), Khách hàng (cyan), Tổng đơn (đen), Đơn chờ (vàng)
- Bảng "5 đơn hàng mới nhất" (badge: Chờ/Giao/Xong) bên cạnh "5 bài viết mới nhất" — layout 2 cột

**Thứ tự middleware hoàn chỉnh:**
```
UseRouting -> UseCors -> UseSwagger -> UseSwaggerUI
-> UseAuthentication -> UseAuthorization -> MapStaticAssets
-> MapControllers (API) -> MapControllerRoute (MVC)
```

**Kiến thức:**  
`[ApiController]` vs `Controller`, `ControllerBase`, HTTP verbs (`HttpGet`/`HttpPost`), `[FromBody]`, DTO, `Ok()`/`NotFound()`/`StatusCode()`, Swagger/OpenAPI, CORS policy, `ThenInclude` cho Eager Loading 3 cấp, phân quyền theo Role với `[Authorize(Roles = "Admin")]`.

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
| CategoriesProducts | 3 | Áo, Quần, Giày dép |
| Products | 6 | Mỗi sản phẩm thuộc 1 danh mục sản phẩm |
| Customers | 3 | Khách hàng mẫu |
| Orders | 4 | Trạng thái: Chờ (0), Đang giao (1), Hoàn thành (2) |

> **Lưu ý bảo mật:** Mật khẩu trong `DbInitializer` lưu thô (plain text) chỉ để học tập.  
> Trong môi trường thực tế phải hash bằng BCrypt hoặc ASP.NET Core Identity.  
> CORS `AllowAnyOrigin()` chỉ dùng trong môi trường học — thực tế phải chỉ rõ domain cụ thể.
