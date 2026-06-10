# BUỔI 8 - Gọi API từ ReactJS & Hook `useEffect`

> **Họ tên:** Quang Văn Trường | **MSV:** 2123170591
> **Môn:** ASP.NET Core | **Giảng viên:** Nguyễn Cao Thái
> **Project:** `cms.frontend` (ReactJS + Vite + TypeScript) + `CMS.Backend` (ASP.NET Core Web API)

---

## 1. Mục tiêu

- Nắm vững cơ chế **`useEffect`** để kiểm soát thời điểm gọi API (Side Effect), tránh vòng lặp render vô hạn.
- Kết hợp **`useState` + `async/await`** quản lý 3 trạng thái: *đang tải* -> *có dữ liệu* / *lỗi*.
- Gọi Web API lấy dữ liệu thật từ SQL Server đổ ra giao diện ReactJS.

---

## 2. Phần thực hành chung (PHẦN 2 trong đề cương) - đã hoàn thành từ Buổi 7

Đề cương Buổi 8 yêu cầu: `blogService.getAllPosts()` -> component `PostList` dùng `useEffect` -> nhúng vào trang chủ.
Tầng này **đã được dựng ở Buổi 7** và vẫn đang chạy:

| Yêu cầu đề cương | Hiện thực trong project |
|------------------|-------------------------|
| `blogService.getAllPosts()` (route `/Posts`) | [`getPosts()`](src/services/blogService.ts) -> `GET /api/posts` |
| `getPostById(id)` (route `/Posts/{id}`) | [`getPostById()`](src/services/blogService.ts) -> `GET /api/posts/{id}` |
| Component `PostList` + `useEffect` | [`NewsSection.tsx`](src/components/sections/NewsSection.tsx) (trang chủ) & [`BlogPage.tsx`](src/pages/Blog/BlogPage.tsx) (`/blog`) |
| Nhúng vào `App` | Route khai báo trong [`App.tsx`](src/App.tsx) |

> Cùng một tư duy với đoạn `PostList.jsx` trong đề cương: `useState([])` -> `useEffect(fetch, [])` ->
> `if (loading) return <spinner/>` -> render danh sách. Chỉ khác: project viết bằng **TypeScript**,
> tách **3 lớp** `axiosClient -> http -> service` và có **công tắc mock** (`VITE_USE_MOCK`).

---

## 3. Bài tập tự làm Buổi 8 (phần MỚI) - Chuyên mục tin tức (Blog Category)

Yêu cầu: lặp lại quy trình **Service -> Component -> Nhúng** cho bảng **`Categories`** (chuyên mục **bài viết**),
gọi tới endpoint **`/Categories`** ở Backend.

> Lưu ý: Phân biệt 2 cấu trúc dữ liệu song song:
> - **`CategoryProduct`** (`/api/categoriesproducts`) - phân loại **sản phẩm** bán hàng (Buổi 7).
> - **`Category`** (`/api/categories`) - phân loại **bài viết / tin tức** (Buổi 8). <- phần này.

### 3.1. Backend - mở "cửa ngõ dữ liệu" JSON

`CategoryController` cũ chỉ trả về **View** (MVC, có `[Authorize]`), ReactJS không gọi được.
Vì vậy tạo mới **`CategoriesController`** trả JSON thuần - theo đúng quy ước plural `...sController` +
`[ApiController]` của project (giống `PostsController`, `CategoriesProductsController`).

| File | Route | Trả về |
|------|-------|--------|
| [`Controllers/CategoriesController.cs`](../TruongCMS_Solution/CMS.Backend/Controllers/CategoriesController.cs) | `GET /api/categories` | `[{ id, name, description }]` (projection từ bảng `Categories`) |
| | `GET /api/categories/{id}` | 1 chuyên mục theo Id |

### 3.2. Frontend - Service, Component, Wiring

| File | Vai trò |
|------|---------|
| [`types/index.ts`](src/types/index.ts) | Thêm type **`BlogCategory`** (`id, name, description`) - tách bạch với `Category` sản phẩm |
| [`services/blogService.ts`](src/services/blogService.ts) | Thêm hàm **`getBlogCategories()`** -> `GET /api/categories`, map JSON Backend -> `BlogCategory`, có nhánh mock |
| [`services/mock/data.ts`](src/services/mock/data.ts) | Thêm **`mockBlogCategories`** để xem offline (`VITE_USE_MOCK=true`) |
| [`components/sections/BlogCategoryList.tsx`](src/components/sections/BlogCategoryList.tsx) | Component **tự gọi API qua `useEffect`** (đúng tư duy Buổi 8), render List Group + trạng thái loading |
| [`pages/Blog/BlogPage.tsx`](src/pages/Blog/BlogPage.tsx) | Nhúng sidebar `BlogCategoryList`; bấm 1 chuyên mục để **lọc** bài viết |

### 3.3. Luồng dữ liệu

```
BlogCategoryList  (useState([]) + useEffect chạy 1 lần khi mount)
      |  getBlogCategories()
blogService.getBlogCategories()   USE_MOCK ? mockBlogCategories : apiGet('/categories')
      |
api/categories  ->  CategoriesController.GetAll()  ->  bảng Categories (SQL Server)
```

---

## 4. Cách chạy & nghiệm thu (Checkpoint)

1. Chạy **`CMS.Backend`** (F5 trong Visual Studio) ở `https://localhost:7152`, chấp nhận chứng chỉ HTTPS.
2. Trong `cms.frontend`: đảm bảo `.env` có `VITE_USE_MOCK=false`, rồi `npm run dev` (http://localhost:3000).
3. Mở **`/blog`**:
   - Sidebar **"Chủ đề bài viết"** hiển thị đúng các chuyên mục trong bảng `Categories` của SQL Server
     (mặc định seed: *Tin tức Công nghệ, Đời sống du lịch, Sức khỏe Thể thao, Giáo dục Kỹ năng, Góc lập trình viên*).
   - Khi tải lại trang (F5): khu vực sidebar hiện dòng *"Đang nạp các chuyên mục bài viết..."* trước khi đổ dữ liệu (minh chứng bất đồng bộ).
   - Bấm 1 chủ đề -> danh sách bài viết được lọc theo chuyên mục; bấm **"Tất cả chủ đề"** để bỏ lọc.

> Demo nhanh không cần Backend: đặt `VITE_USE_MOCK=true` (hoặc tạo `.env.local`) để chạy bằng mock data.

---

## 5. Troubleshooting (theo đề cương)

| Hiện tượng | Nguyên nhân | Cách sửa |
|------------|-------------|----------|
| Màn hình nhấp nháy / treo trình duyệt | `useEffect` thiếu mảng phụ thuộc `[]` ở cuối -> render vô hạn | Thêm `, []` vào cuối `useEffect(...)` |
| Sidebar trống dù Backend chạy | Bảng `Categories` chưa có dữ liệu, hoặc gọi nhầm `/categoriesproducts` | Kiểm tra seed trong `DbInitializer`; route phải là `/categories` |
| Lỗi CORS trên Console (F12) | Backend chưa bật CORS cho cổng ReactJS | Đã cấu hình `AllowAll` trong `Program.cs` (Buổi 6) - chạy lại Backend |
| Ngày tháng dính `T00:00:00` | `DateTime` từ SQL kèm múi giờ | Dùng `formatDate()` (đã có sẵn `new Date(...).toLocaleDateString('vi-VN')`) |

---

## 6. Kết quả đạt được

- ReactJS gọi **`/api/categories`** bằng `useEffect`, hiển thị chuyên mục tin tức lấy thật từ SQL Server.
- Tách bạch rõ **chuyên mục bài viết** (`Category`) với **danh mục sản phẩm** (`CategoryProduct`).
- Sidebar chuyên mục còn **lọc được bài viết**, tái sử dụng đúng kiến trúc Service nhiều lớp của Buổi 7.
