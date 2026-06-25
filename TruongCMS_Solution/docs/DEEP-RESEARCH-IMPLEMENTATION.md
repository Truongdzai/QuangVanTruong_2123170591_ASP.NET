# Nâng cấp hệ thống theo Báo cáo nghiên cứu chuyên sâu

Tài liệu này đối chiếu **deep-research-report.md** với phần đã hiện thực trong cả
Backend (ASP.NET Core 10) và Frontend (React + Vite + TypeScript).

> Tóm tắt: đã triển khai **7/7 nhóm chức năng** và xử lý đủ **15/15 test case**.
> Backend build sạch (0 error), frontend `tsc --noEmit` + `vite build` sạch.
> Đã smoke-test các luồng chính bằng API thật (xem mục “Kết quả kiểm thử”).

---

## 1. Bảy nhóm chức năng

### 1. Quản lý Sản phẩm & Kho hàng (Catalog & Inventory)
| Yêu cầu | Hiện thực |
|---|---|
| Cây danh mục Cha–Con | `CategoryProduct.ParentId` + self-FK (`Restrict`) — [CategoryProduct.cs](../CMS.Data/Entities/CategoryProduct.cs) |
| Thương hiệu/nhãn hàng | Entity `Brand`, `Product.BrandId`, API `GET /api/brands` — [Brand.cs](../CMS.Data/Entities/Brand.cs), [ProductsController.cs](../CMS.Backend/Controllers/ProductsController.cs) |
| Thuộc tính biến thể (màu HEX, size) | `ProductVariant` (Color hex, ColorName, Size) — [ProductVariant.cs](../CMS.Data/Entities/ProductVariant.cs) |
| Biến thể SKU (giá/tồn/ảnh riêng) | `ProductVariant.PriceOverride / StockQuantity / ImageUrl`, unique `(ProductId,Color,Size)` |
| Đồng bộ tồn kho + cảnh báo | Trừ kho nguyên tử khi đặt; admin BI cảnh báo “sắp hết (≤5)” / “hết hàng” — [ReportsController.cs](../CMS.Backend/Controllers/ReportsController.cs) |
| Quản trị biến thể | [VariantController.cs](../CMS.Backend/Controllers/VariantController.cs) + view `Views/Variant/Manage.cshtml` |

### 2. Quản lý Đơn hàng & Vận chuyển (Order & Logistics)
| Yêu cầu | Hiện thực |
|---|---|
| Quy trình trạng thái tự động | `OrderStatusFlow` máy trạng thái 6 bước, một chiều — [Order.cs](../CMS.Data/Entities/Order.cs) |
| Tích hợp vận chuyển (GHTK/GHN/VTP) | `IShippingService` (mock GHTK: sinh mã vận đơn, tính phí) — [ShippingService.cs](../CMS.Backend/Services/ShippingService.cs) |
| Webhook trạng thái giao hàng | `POST /api/shipping/callback` idempotent + token — [ShippingController.cs](../CMS.Backend/Controllers/ShippingController.cs) |
| Hóa đơn điện tử (PDF) | `GET /api/orders/{id}/invoice` xuất HTML khổ A4, in/lưu PDF — [OrdersController.cs](../CMS.Backend/Controllers/OrdersController.cs) |
| Hoàn tiền khi trả hàng | `OrderWorkflowService` hoàn kho + đánh dấu Refund — [OrderWorkflowService.cs](../CMS.Backend/Services/OrderWorkflowService.cs) |

### 3. Quản lý Thanh toán (Payment Gateway)
| Yêu cầu | Hiện thực |
|---|---|
| COD + VNPAY | `Order.PaymentMethod`, chọn ở Checkout React |
| Ký HMAC-SHA512 / chống giả mạo | `VnPayService` ký + verify chữ ký — [VnPayService.cs](../CMS.Backend/Services/VnPayService.cs) |
| IPN webhook idempotent | `GET /api/payments/vnpay-ipn` (unique `TxnRef`, xử lý 1 lần) — [PaymentsController.cs](../CMS.Backend/Controllers/PaymentsController.cs) |
| Cổng demo offline | `/payment-demo` (ngân hàng giả lập) khi chưa có TmnCode thật |

### 4. Quản lý Khách hàng & Phân quyền (CRM & IAM)
| Yêu cầu | Hiện thực |
|---|---|
| Lịch sử mua, địa chỉ mặc định | `GET/PUT /api/customers/me` — [CustomersController.cs](../CMS.Backend/Controllers/CustomersController.cs) |
| Wishlist | Entity `WishlistItem`, `WishlistController`, React `WishlistContext` + `/wishlist` |
| RBAC (Admin/Kho/CSKH/Kế toán) | Seed role `Staff/Support/Accountant`; `[Authorize(Roles=...)]` từng controller |
| Loyalty (Bạc/Vàng/Kim cương) | `Customer.LoyaltyPoints` + `MembershipTier`; cộng điểm khi đơn Thành công |

### 5. Quản lý Khuyến mãi & Marketing (Marketing Engine)
| Yêu cầu | Hiện thực |
|---|---|
| Coupon kèm điều kiện | `DiscountCode.MinOrderAmount`, `MaxUses`, `MaxUsesPerCustomer` + bảng `DiscountUsage` |
| Kiểm tra điều kiện ở server | `DiscountsController.KiemTraMaAsync` — [DiscountsController.cs](../CMS.Backend/Controllers/DiscountsController.cs) |
| Flash Sale theo khung giờ | `FlashSale`/`FlashSaleItem`, `PricingService` check giờ server — [PricingService.cs](../CMS.Backend/Services/PricingService.cs) |
| Quản trị Flash Sale | [FlashSaleController.cs](../CMS.Backend/Controllers/FlashSaleController.cs) + views; API `GET /api/flashsales/active` |

### 6. Báo cáo & Thống kê (Business Intelligence)
| Yêu cầu | Hiện thực |
|---|---|
| Doanh thu ngày/tháng + tăng trưởng | `ReportsController` + Chart.js (`Views/Reports/Index.cshtml`) |
| Top bán chạy / tồn kho lâu | Truy vấn `OrderDetails` gom nhóm + sản phẩm chưa bán |
| Retention (mua lần 2) | Tỷ lệ khách có ≥2 đơn không hủy |

### 7. Kỹ thuật & Hạ tầng (System & Operations)
| Yêu cầu | Hiện thực |
|---|---|
| Background Jobs (Hangfire) | Hủy đơn quá hạn (5'/lần), tắt mã hết hạn (hằng ngày), gửi email — [BackgroundJobs.cs](../CMS.Backend/Services/BackgroundJobs.cs), dashboard `/hangfire` (chỉ Admin) |
| Caching (Redis/Memory) | `ICacheService` Cache-Aside; Redis khi có `Redis:ConnectionString`, không thì RAM — [CacheService.cs](../CMS.Backend/Services/CacheService.cs) |
| Logging (Serilog) | Console + file JSON `logs/` (CompactJsonFormatter) — [Program.cs](../CMS.Backend/Program.cs) |

---

## 2. Đối chiếu 15 Test Case

| # | Test case | Cách xử lý | Vị trí |
|---|---|---|---|
| 1 | Chọn màu → đổi ảnh | Biến thể có `ImageUrl` riêng theo màu; React đổi ảnh khi chọn màu | `ProductDetailPage.tsx`, seed ảnh theo màu trong `DbInitializer` |
| 2 | Biến thể hết hàng → khóa nút riêng | Tồn kho theo từng SKU; size hết hàng bị disable, màu/size khác vẫn mua | `ProductDetailPage.tsx` (effectiveStock) |
| 3 | Size lớn giá cao | `ProductVariant.PriceOverride`; giá hiển thị đổi theo biến thể | seed +5%/nấc size; verified: M=126, L=132, XL=138 |
| 4 | Đổi SL → tổng tự cập nhật | React state ở `CartContext`/`CartPage`, không reload | `CartContext.tsx` |
| 5 | Vượt tồn kho → chặn | `addToCart`/`updateQty` chặn client + Backend kiểm tra lại | `CartContext.tsx` + `OrdersController` |
| 6 | Race condition (mua đồng thời) | `UPDATE ... WHERE StockQuantity >= qty` (khóa dòng) → người sau nhận 409 | `OrdersController.CreateOrder` |
| 7 | Mã yêu cầu đơn tối thiểu | Server kiểm tra `MinOrderAmount` với tạm tính thật | `DiscountsController` — verified 450 từ chối / 520 nhận |
| 8 | Mã 1 lần/khách | Bảng `DiscountUsage` đếm theo email; `MaxUsesPerCustomer` | `OrdersController` + `DiscountsController` |
| 9 | Hết Flash Sale → về giá gốc | `PricingService` so `DateTime.Now` (giờ server) mỗi lần tính giá | `PricingService`, `FlashSalesController` |
| 10 | Hủy thanh toán → giữ giỏ | Return URL `status=cancelled`; React giữ giỏ + cho thử lại | `PaymentResultPage.tsx`, `CheckoutPage.tsx` |
| 11 | Thành công muộn (IPN) | Webhook IPN idempotent cập nhật đơn dù khách tắt trình duyệt | `PaymentsController` — verified paymentStatus=1 |
| 12 | Luồng trạng thái một chiều | `OrderStatusFlow.CanTransition` chặn lùi; Admin chỉ thấy bước hợp lệ | `OrderWorkflowService`, `Views/Order/Details.cshtml` |
| 13 | Email tự động (Hangfire) | `BackgroundJobs.GuiEmailXacNhanDon` enqueue ngay khi đặt | `OrdersController` + `BackgroundJobs` |
| 14 | Chặn truy cập trái phép (403/401) | Cookie events trả 401/403 JSON cho `/api`; `[Authorize]` từng API | `Program.cs` — verified 401 khi gọi `/me` không token |
| 15 | Hết hạn token → refresh ngầm | Access 15', Refresh 7 ngày; axios tự refresh khi 401 | `axiosClient.ts`, `TokenService`, `CustomersController` |

---

## 3. Phân tích rủi ro bảo mật (theo báo cáo)

- **Concurrency**: trừ kho nguyên tử + transaction (không bán quá tồn).
- **IPN idempotent**: `TxnRef` unique + kiểm chữ ký HMAC + đối chiếu số tiền.
- **IAM**: Cookie (Admin MVC) + JWT Bearer (khách React) song song; API trả 403/401.
- **OWASP**: rate limiting `auth` (10 lần/phút/IP) chống brute-force; EF Core tham số hóa
  (chống SQL injection); antiforgery token trên form admin.
- **Marketing**: mọi điều kiện mã giảm/flash sale kiểm tra ở **server** theo giờ server.
- **Caching**: Cache-Aside cho danh mục/flash sale/bán chạy (fallback DB khi cache lỗi).
- **Logging**: Serilog JSON, tách mức log, sẵn sàng đẩy lên Seq/ELK.

---

## 4. Cấu hình (appsettings.json)

| Khối | Ý nghĩa |
|---|---|
| `Jwt` | Khóa ký, thời hạn access (15') / refresh (7 ngày) |
| `VnPay` | `TmnCode` trống = chạy cổng demo `/payment-demo`; điền vào để dùng sandbox thật |
| `Payment.UnpaidOrderTtlMinutes` | Đơn VNPAY quá hạn này chưa trả → Hangfire tự hủy + hoàn kho |
| `Shipping` | Ngưỡng miễn phí ship, phí cơ bản, token webhook |
| `Loyalty.AmountPerPoint` | Bao nhiêu tiền = 1 điểm tích lũy |
| `Redis.ConnectionString` | Có giá trị → bật Redis cache; trống → cache RAM |

---

## 5. Kết quả kiểm thử (smoke test API thật)

```
✓ GET /api/products/1        → 9 biến thể SKU, flashPrice=84 (gốc 120), giá size tăng dần
✓ GET /api/flashsales/active → "FLASH SALE CUỐI TUẦN" kèm serverTime
✓ POST /api/discounts/validate (450) → từ chối ; (520) → áp 10%        [test 7]
✓ POST /api/orders (variant)  → trừ kho nguyên tử 5→3, phí ship 22     [test 3,6]
✓ POST /api/orders (oversell) → HTTP 409 "Số lượng ... không đủ"       [test 6]
✓ POST /api/customers/register→ accessToken + refreshToken + mã chào mừng [test 15]
✓ GET  /api/customers/me      → loyaltyPoints + hạng Bạc               [mục 4]
✓ GET  /api/customers/me (no token) → 401                              [test 14]
✓ POST /api/customers/refresh-token → access token mới                 [test 15]
✓ POST /api/orders/5/cancel   → hoàn kho 3→5, trạng thái "Đã hủy"      [test 12]
✓ VNPAY demo + IPN ipnOnly    → paymentStatus=1 dù "tắt trình duyệt"   [test 11]
```

## 5b. Bổ sung theo yêu cầu (đợt 2)

| Yêu cầu | Hiện thực | Vị trí |
|---|---|---|
| Chọn màu → ảnh đại diện đổi ngay | Ảnh riêng của biến thể màu được đưa lên đầu danh sách ảnh; click màu → ảnh chính đổi | `ProductDetailPage.tsx` |
| Giỏ hàng hiện màu + kích cỡ | Ô màu trực quan + tên màu + size từng dòng | `CartPage.tsx`, `CartDrawer.tsx` |
| Đổi mật khẩu (cũ + mới ×2) | `POST /api/customers/change-password` verify mật khẩu cũ; form trong AccountPage nhập mới 2 lần | `CustomersController.cs`, `AccountPage.tsx` |
| SMTP Gmail thật | `appsettings.json` → qvantruong205@gmail.com + app password; đã test gửi thành công | `appsettings.json`, `EmailService.cs` |
| Admin thêm biến thể dễ + ảnh URL/upload | VariantController nhận `imageUrl` HOẶC `imageFile`; bảng quản lý inline | `VariantController.cs`, `Views/Variant/Manage.cshtml` |
| Ảnh sản phẩm: URL hoặc upload | Product Create/Edit có ô upload file + ô dán URL (dùng cả hai) | `ProductController.cs`, `Views/Product/Create+Edit.cshtml` |
| Footer động | `FooterLink` + `SiteSetting` + `FooterController` + `GET /api/footer`; React đọc động, fallback tĩnh | `FooterController.cs`, `Footer.tsx` |
| Phân trang + tìm kiếm + lọc (admin) | Product Index: ô tìm theo tên/mô tả + lọc danh mục + phân trang giữ tham số | `ProductController.cs`, `Views/Product/Index.cshtml` |
| Chặn giá âm | Validation server-side ở Product/Variant + `[Range]` ở DiscountCode/FlashSale | `ProductController`, `VariantController`, entities |

Kết quả test đợt 2: footer API 4 cột ✓, tìm kiếm ✓, biến thể ảnh theo màu ✓, đổi mật khẩu (sai/đúng mật khẩu cũ) ✓, SMTP Gmail gửi thành công ✓.

## 5c. Admin thân thiện (đợt 3)

| Yêu cầu | Hiện thực | Vị trí |
|---|---|---|
| Bộ chọn MÀU trực quan (thay vì gõ tên) | Partial `_ColorSizePicker`: bấm ô màu của shop + thêm màu tùy ý (input color); ghi CSV hex vào hidden `Colors` | `Views/Shared/_ColorSizePicker.cshtml` |
| Chọn SIZE bằng chip | Chip size preset (X-Small…XX-Large) + thêm size tùy ý; ghi CSV vào hidden `Sizes` | (cùng partial) |
| Đồng bộ Create/Edit sản phẩm | Cả 2 trang dùng CHUNG partial, Edit pre-select màu/size cũ | `Product/Create.cshtml`, `Product/Edit.cshtml` |
| Phân trang + tìm kiếm các trang quản lý | Partial dùng chung `_AdminSearch` + `_AdminPager`; thêm search + pagination vào Danh mục tin, Danh mục SP, Khách hàng, Thành viên, Bài viết | các controller `Index` + `Views/*/Index.cshtml` |

Kết quả test đợt 3 (đăng nhập admin thật): Create/Edit có color picker + size chip ✓ (Edit pre-select đúng `#F50606,#06CAF5,#F506A4` / `Medium,Large,X-Large`), 4 trang quản lý có ô tìm kiếm ✓, lọc khách hàng 6→1 khi tìm "vnpay" ✓.

## 6. Chưa làm (ngoài phạm vi môn học / cần hạ tầng ngoài)

- CI/CD pipeline (GitHub Actions), Docker/K8s, data warehouse/Power BI — báo cáo xếp
  effort cao, cần hạ tầng triển khai riêng.
- Tích hợp **API thật** của GHTK/GHN/VNPAY cần tài khoản merchant; hệ thống đã tách
  `IShippingService`/`IVnPayService` nên chỉ cần thay implementation, không sửa controller.
