# BUỔI 7 – Kết nối Frontend ReactJS với ASP.NET Core Web API

> **Họ tên:** Quang Văn Trường | **MSV:** 2123170591
> **Môn:** ASP.NET Core | **Giảng viên:** Nguyễn Cao Thái
> **Project:** `cms.frontend` (ReactJS + Vite + TypeScript)

---

## 1. Mục tiêu

Chuyển tư duy từ render phía Server (Razor View) sang render phía Client (ReactJS),
và **lấy dữ liệu thật từ Database** thông qua Web API đã xây ở Buổi 6.

Cụ thể, buổi này hoàn thiện **tầng kết nối dữ liệu** cho toàn bộ website bán hàng + tin tức:

- Cấu hình một **axios client tập trung** trỏ tới ASP.NET Core (`https://localhost:7152/api`).
- Xây **tầng Service** đứng giữa giao diện và API: gọi HTTP rồi *map* JSON của Backend
  (camelCase, theo entity trong `CMS.Data`) sang kiểu dữ liệu của Frontend.
- Dùng **`useEffect`** trong các page/component để gọi Service và đổ dữ liệu ra giao diện.
- Có **công tắc Mock** (`VITE_USE_MOCK`) để chạy thử giao diện khi Backend chưa bật.
- **Mở rộng – Tin tức/Blog:** danh sách bài viết mới nhất ở trang chủ, trang danh sách Blog
  và trang chi tiết bài viết.

---

## 2. Kiến trúc luồng dữ liệu

```
  Component / Page  (useEffect + useState)
        │  gọi hàm service, ví dụ getPosts()
        |
  *Service  (blogService, productService, categoryService, contactService)
        │  USE_MOCK ? trả mock data : gọi apiGet / apiPost
        │  map JSON Backend  ->  type Frontend
        |
  services/http.ts   (apiGet, apiPost, delay, resolveImageUrl)
        
  api/axiosClient.ts (1 axios instance dùng chung)
        │  baseURL = VITE_API_BASE_URL
        │  interceptor: bóc sẵn response.data + log lỗi tập trung
        |
  ASP.NET Core Web API   https://localhost:7152/api/...
        
  SQL Server (ThaiCMS_DB)
```

**Vì sao tách 3 lớp `axiosClient -> http -> service`?**
Để mọi nơi gọi API đi chung một cấu hình (base URL, timeout, header, xử lý lỗi),
không lặp lại đường dẫn `https://localhost:7152/api/...` rải rác trong code,
và khi đổi cổng Backend chỉ phải sửa **một chỗ** trong `.env`.

---

## 3. Danh sách file đã làm

### A. Hạ tầng kết nối (Phần 1 & 2)

| File | Vai trò |
|------|---------|
| `src/configs/env.ts` | Đọc biến môi trường `VITE_*` (`API_BASE_URL`, `USE_MOCK`, `SITE_*`) kèm giá trị mặc định |
| `src/api/axiosClient.ts` | Tạo **một** axios instance dùng chung; interceptor bóc thẳng `response.data` và log lỗi |
| `src/services/http.ts` | Helper tái sử dụng: `apiGet` / `apiPost`, cờ `USE_MOCK`, `delay()` (giả lập trễ mạng), `resolveImageUrl()` (ghép URL ảnh tương đối về Backend) |

### B. Tầng Service (gọi API + map dữ liệu)

| File | Hàm chính | Route Backend |
|------|-----------|---------------|
| `src/services/productService.ts` | `getProducts`, `getProductById`, `getProductBySlug`, `getProductsByCategory` | `/products`, `/products/{id}`, `/products/categoryproduct/{categoryId}` |
| `src/services/categoryService.ts` | `getCategories` | `/categoriesproducts` |
| `src/services/blogService.ts` | `getPosts`, `getPostById`, `getPostBySlug` | `/posts`, `/posts/{id}` |
| `src/services/contactService.ts` | `submitEmail`, `submitContact` | `POST /newsletter`, `POST /contact` |
| `src/services/mock/` | `mockProducts`, `mockCategories`, `mockPosts`… – dữ liệu giả khi `USE_MOCK=true` |

### C. Page / Component hiển thị dữ liệu (dùng `useEffect`)

| File | Route | Gọi gì |
|------|-------|--------|
| `src/pages/Home/HomePage.tsx` | `/` | `getProducts()` -> 2 khối "Hàng mới về" / "Bán chạy nhất" |
| `src/pages/Products/ProductsPage.tsx` | `/products` | `getProducts()` + `getCategories()` (lưới sản phẩm & bộ lọc) |
| `src/pages/Products/ProductDetailPage.tsx` | `/products/:slug` | `getProductBySlug()` + `getProductsByCategory()` ("có thể bạn cũng thích") |
| `src/pages/Contact/ContactPage.tsx` | `/contact` | `submitContact()` (POST form liên hệ) |
| **`src/components/sections/NewsSection.tsx`** | (trong `/`) | `getPosts(3)` – 3 tin mới nhất ở trang chủ |
| **`src/pages/Blog/BlogPage.tsx`** | `/blog` | `getPosts(20)` – danh sách bài viết |
| **`src/pages/Blog/BlogDetailPage.tsx`** | `/blog/:id` | `getPostById()` – chi tiết bài viết |

> Các route được khai báo trong `src/App.tsx` (React Router, lazy-load từng page).

---

## 4. Phần mở rộng – Tin tức / Blog

Luồng hoàn chỉnh của tính năng Tin tức:

1. **Trang chủ** – `NewsSection` chạy `useEffect -> getPosts(3)`; nếu chưa có bài (Backend trống)
   thì ẩn cả khu vực (`return null`).
2. **Trang Blog** (`/blog`) – `getPosts(20)` đổ ra lưới thẻ bài viết, mỗi thẻ link sang chi tiết.
3. **Chi tiết** (`/blog/:id`) – lấy `id` từ URL bằng `useParams`, gọi `getPostById(id)`,
   tách `content` theo dấu xuống dòng để render từng đoạn `<p>`.

`blogService` map dữ liệu Backend (`Post` trong `CMS.Data`) sang type `Post` của Frontend,
trong đó `slug = String(id)` (Backend chưa có trường slug) và `excerpt` được cắt từ `content`.

---

## 5. Ánh xạ Frontend <-> Backend API

Tất cả endpoint dưới đây nối tiếp sau base URL `VITE_API_BASE_URL` (mặc định `…/api`):

| Hàm Frontend | HTTP | Endpoint |
|--------------|------|----------|
| `getProducts` | GET | `/products` |
| `getProductById` | GET | `/products/{id}` |
| `getProductsByCategory` | GET | `/products/categoryproduct/{categoryId}` |
| `getCategories` | GET | `/categoriesproducts` |
| `getPosts` | GET | `/posts` |
| `getPostById` | GET | `/posts/{id}` |
| `submitEmail` | POST | `/newsletter` |
| `submitContact` | POST | `/contact` |

---

## 6. Cấu hình & cách chạy

### 6.1. Biến môi trường (`.env`)

```ini
# Base URL của ASP.NET Core Web API (CMS.Backend)
VITE_API_BASE_URL=https://localhost:7152/api

# true  = chạy bằng MOCK DATA (không cần Backend)
# false = gọi Backend thật ở cổng 7152
VITE_USE_MOCK=false
```

> Quy ước trong `env.ts`: chỉ khi `VITE_USE_MOCK=false` mới gọi Backend thật;
> mọi giá trị khác (kể cả khi bỏ trống) đều coi là **mock = true**.

### 6.2. Lệnh chạy

```bash
npm install        # cài dependencies (lần đầu)
npm run dev        # chạy Vite dev server tại http://localhost:3000
npm run build      # build production (tsc -b && vite build)
npm run type-check # kiểm tra lỗi TypeScript (tsc --noEmit)
```

### 6.3. Hai cách demo

| Cách | Thiết lập | Phù hợp khi |
|------|-----------|-------------|
| **Mock** | `VITE_USE_MOCK=true` | Xem giao diện ngay, chưa bật Backend |
| **Backend thật** | `VITE_USE_MOCK=false` + chạy `CMS.Backend` ở `:7152` + có dữ liệu trong SQL | Demo luồng dữ liệu thật từ Database |

> Khi dùng Backend thật, nhớ chạy `CMS.Backend` (F5 trong Visual Studio) **trước**, và chấp nhận
> chứng chỉ HTTPS `localhost`. Nếu trang Tin tức/Sản phẩm trống nghĩa là bảng `Posts`/`Products`
> trong SQL chưa có dữ liệu — hãy nhập vài dòng mẫu (xem Buổi 2).

---

## 7. Kết quả đạt được

- ReactJS hiển thị **sản phẩm, danh mục, tin tức** lấy trực tiếp từ ASP.NET Core Web API.
- Một tầng Service gọn gàng, dễ mở rộng; đổi cổng Backend chỉ sửa `.env`.
- Công tắc Mock giúp demo giao diện kể cả khi chưa có Backend.
- Hoàn thành đúng mục tiêu đề cương: *"Sử dụng giao diện ReactJS gọi WebAPI"*.
