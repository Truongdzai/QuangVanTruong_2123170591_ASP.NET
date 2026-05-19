# Buổi 1 — Khởi tạo cấu trúc dự án CMS Full-Stack

| Thông tin | Chi tiết |
|-----------|----------|
| **Sinh viên** | Quang Văn Trường |
| **MSV** | 2123170591 |
| **Solution** | `TruongCMS_Solution` |
| **Môn học** | ASP.NET Core (Giảng Viên: Nguyễn Cao Thái) |

---

## 1. Mục tiêu buổi học

Sau Buổi 1, bạn có thể:

- Tự thiết lập **Solution 3 lớp** : Data -> Backend -> Frontend (React, Buổi 7).
- Định nghĩa **8 thực thể (Entity)** cho hệ thống CMS + bán hàng.
- Hiểu mô hình **MVC**: Controller xử lý logic -> View hiển thị HTML.
- Tạo trang danh sách bằng **dữ liệu giả (mock data)** trong code — chưa cần SQL Server.

---

## 2. Kiến trúc Solution 3 lớp

```
TruongCMS_Solution/
├── CMS.Data/              <- Lớp 1: Entity (mô hình dữ liệu)
├── CMS.Backend/           <-> Lớp 2: ASP.NET Core MVC + Web API (sau này)
└── cms.frontend/          <-> Lớp 3: ReactJS (Buổi 7)
```

| Project | Công nghệ | Vai trò |
|---------|-----------|---------|
| **CMS.Data** | Class Library (.NET 10) | Chứa các class `Entity` ánh xạ bảng DB |
| **CMS.Backend** | ASP.NET Core MVC | Trang Admin, API, gọi sang CMS.Data |
| **cms.frontend** | React (Create React App) | Giao diện người xem (Buổi 7+) |

**Luồng dữ liệu Buổi 1:**

```
Controller (mock List trong code) -> View (Razor .cshtml) ->Trình duyệt
```

---

## 3. Các bước thực hiện (theo giáo trình)

### Bước 1 — Tạo Blank Solution

1. Visual Studio 2022 → **Create a new project** -> **Blank Solution**.
2. Tên: `TruongCMS_Solution`.

### Bước 2 — Tạo project CMS.Data

1. Chuột phải Solution → **Add** → **New Project** → **Class Library**.
2. Tên: `CMS.Data`, framework: **.NET 10**.
3. Tạo thư mục `Entities`, thêm 8 class (xem mục 4).

### Bước 3 — Tạo project CMS.Backend

1. **Add** -> **New Project** -> **ASP.NET Core Web App (Model-View-Controller)**.
2. Tên: `CMS.Backend`.
3. Chuột phải `CMS.Backend` -> **Add** → **Project Reference** -> chọn `CMS.Data`.
4. Chuột phải `CMS.Backend` -> **Set as Startup Project**.

### Bước 4 — Tạo project React (tùy chọn Buổi 1)

```powershell
cd d:\asp.NET\TruongCMS_Solution
npx create-react-app cms.frontend
```

Thêm vào Solution: **Add** -> **Existing Web Site** -> chọn folder `cms.frontend`.

### Bước 5 — Demo MVC với mock data

Tạo Controller + View cho **Category**, **Post**, **User** (xem mục 5–6).

---

## 4. Danh sách 8 Entity (CMS.Data/Entities)

| # | File | Bảng SQL (Buổi 2) | Mô tả 
|---|------|-------------------|--------
| 1 | `Category.cs` | Categories | Danh mục tin tức 
| 2 | `Post.cs` | Posts | Bài viết (Title, Content, CategoryId…) 
| 3 | `User.cs` | Users | Tài khoản quản trị |
| 4 | `CategoryProduct.cs` | CategoriesProducts | Danh mục sản phẩm 
| 5 | `Product.cs` | Products | Sản phẩm 
| 6 | `Customer.cs` | Customers | Khách mua hàng 
| 7 | `Order.cs` | Orders | Đơn hàng |
| 8 | `OrderDetail.cs` | OrderDetails | Chi tiết đơn hàng 

**Quan hệ chính:**

- `Category` **1 — n** `Post`
- `CategoryProduct` **1 — n** `Product`
- `Customer` **1 — n** `Order`
- `Order` **1 — n** `OrderDetail`
- `Product` được tham chiếu trong `OrderDetail`

---

## 5. Controller & Mock Data (Buổi 1)

### CategoryController

- **URL:** `/Category` hoặc `/Category/Index`
- **Mock:** 3–5 danh mục tin (vd: Tin Công nghệ, Giáo dục…)
- **View:** `Views/Category/Index.cshtml` — bảng HTML

### PostController

- **URL:** `/Post`
- **Mock:** ít nhất 2–3 bài viết trong `List<Post>` tĩnh
- **Action `Details(int id)`:** xem chi tiết 1 bài (`/Post/Details/1`)
- **View:** `Views/Post/Index.cshtml` (card), `Views/Post/Details.cshtml`

### UserController

- **URL:** `/User`
- **Mock:** 2–3 user (Admin, Editor…)
- **View:** `Views/User/Index.cshtml` — **không** hiển thị mật khẩu

### HomeController

- **URL:** `/` — trang chủ, link tới Category / Post / User

---

## 6. Cấu trúc thư mục quan trọng (sau Buổi 1)

```
CMS.Backend/
├── Controllers/
│   ├── HomeController.cs
│   ├── CategoryController.cs    <- mock → Buổi 2: DB
│   ├── PostController.cs
│   └── UserController.cs
├── Views/
│   ├── Home/Index.cshtml
│   ├── Category/Index.cshtml
│   ├── Post/Index.cshtml
│   ├── Post/Details.cshtml
│   └── User/Index.cshtml
├── Program.cs
└── appsettings.json

CMS.Data/
└── Entities/
    ├── Category.cs
    ├── Post.cs
    ├── User.cs
    └── ... (5 entity còn lại)
```

---

## 7. Khái niệm MVC cần nắm

| Thành phần | Vai trò | Ví dụ trong bài |
|------------|---------|-----------------|
| **Model** | Dữ liệu | Class `Category`, `Post` trong CMS.Data |
| **View** | Giao diện | `Index.cshtml`, `@model IEnumerable<Category>` |
| **Controller** | Điều phối | `CategoryController.Index()` -> `return View(list)` |

**Routing mặc định:** `{controller}/{action}/{id?}`

- `/Post/Details/5` -> `PostController.Details(5)`

---

## 8. Chạy thử & Checkpoint Buổi 1

### Chạy project

1. Set **CMS.Backend** là Startup Project.
2. Nhấn **F5**.
3. Trình duyệt mở `https://localhost:xxxx/`.

### Checkpoint (đạt yêu cầu khi)

| Kiểm tra | Kết quả mong đợi |
|----------|------------------|
| `/Category` | Bảng danh mục hiển thị đúng tên từ mock |
| `/Post` | Danh sách card bài viết |
| `/Post/Details/1` | Trang chi tiết 1 bài |
| `/User` | Bảng thành viên, không có cột mật khẩu |
| Build Solution | 0 Error |

### Bài tập tự rèn (Challenge)

1. Tự tạo `PostController` + `Index.cshtml` (>= 2 bài viết).
2. Tự tạo `UserController` + View (>= 2 user).
3. So sánh hiển thị chuỗi ngắn (`Name`) vs chuỗi dài (`Content`).

---

## 9. Lỗi thường gặp (Buổi 1)

| Lỗi | Nguyên nhân | Cách xử lý |
|-----|-------------|------------|
| `The type or namespace name 'CMS.Data' could not be found` | Chưa Add Project Reference | Reference `CMS.Data` vào `CMS.Backend` |
| View trống / lỗi model | Sai `@model` hoặc chưa truyền `View(data)` | Khớp kiểu `@model` với `return View(...)` |
| 404 Details | Id không có trong mock | Dùng Id có trong `List` mock |
| Trang Welcome mặc định | Sai URL | Gõ thêm `/Category`, `/Post` |

---

## 10. Chuẩn bị cho Buổi 2

Buổi 2 sẽ:

- Cài **Entity Framework Core** + SQL Server.
- Thay mock bằng `_context.Categories.ToListAsync()`.
- Tạo database **TruongCMS_DB** bằng Migration.


## 11. Tài liệu tham khảo

- Giáo trình: `chuyen-de-ASP.NET.docx` — Phần 1, Buổi 1.
- [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- [Razor syntax](https://learn.microsoft.com/aspnet/core/mvc/views/overview)
