// BUỔI 3: TRUY VẤN LINQ & THAO TÁC DỮ LIỆU CHUYÊN SÂU


using CMS.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data;

/// <summary>
/// DbContext: EF Core dùng class này để biết có những bảng nào và quan hệ giữa chúng.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Constructor nhận cấu hình từ Program.cs (connection string, provider SQL Server).
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Mỗi DbSet = 1 bảng trong database TruongCMS_DB
    public DbSet<Category> Categories => Set<Category>();                    // Danh mục tin
    public DbSet<Post> Posts => Set<Post>();                                  // Bài viết
    public DbSet<User> Users => Set<User>();                                  // Thành viên quản trị
    public DbSet<CategoryProduct> CategoriesProducts => Set<CategoryProduct>(); // Danh mục SP
    public DbSet<Product> Products => Set<Product>();                         // Sản phẩm
    public DbSet<Customer> Customers => Set<Customer>();                      // Khách hàng
    public DbSet<Order> Orders => Set<Order>();                               // Đơn hàng
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();             // Chi tiết đơn

    // Buổi 11 — 3 bảng mới: banner trang chủ, mã giảm giá (sale), phiếu nhập kho
    public DbSet<Banner> Banners => Set<Banner>();                            // Banner động
    public DbSet<DiscountCode> DiscountCodes => Set<DiscountCode>();          // Mã giảm giá
    public DbSet<StockReceipt> StockReceipts => Set<StockReceipt>();          // Phiếu nhập kho

    // ── Nâng cấp theo BÁO CÁO NGHIÊN CỨU CHUYÊN SÂU (deep-research-report) ──
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();    // Biến thể SKU (Màu × Size)
    public DbSet<Brand> Brands => Set<Brand>();                               // Thương hiệu
    public DbSet<FlashSale> FlashSales => Set<FlashSale>();                   // Chương trình flash sale
    public DbSet<FlashSaleItem> FlashSaleItems => Set<FlashSaleItem>();       // Sản phẩm trong flash sale
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();          // Sản phẩm yêu thích
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>(); // Giao dịch thanh toán (IPN)
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();          // Refresh token JWT
    public DbSet<DiscountUsage> DiscountUsages => Set<DiscountUsage>();       // Lịch sử dùng mã giảm giá
    public DbSet<FooterLink> FooterLinks => Set<FooterLink>();               // Liên kết footer động
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();            // Cấu hình site (key-value)

    /// <summary>
    /// Cấu hình quan hệ khóa ngoại, quy tắc xóa (Restrict / Cascade).
    /// Gọi khi EF tạo migration hoặc map entity.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product thuộc 1 CategoryProduct; không cho xóa danh mục nếu còn sản phẩm
        modelBuilder.Entity<Product>()
            .HasOne(p => p.CategoryProduct)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Post thuộc 1 Category
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order có nhiều OrderDetail; xóa Order thì xóa luôn chi tiết
        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderDetails)
            .WithOne(d => d.Order)
            .HasForeignKey(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Cấu hình các bảng mới theo báo cáo nghiên cứu ────────────────────

        // Biến thể SKU: xóa Product thì xóa luôn biến thể;
        // mỗi tổ hợp (Sản phẩm, Màu, Size) chỉ tồn tại 1 lần
        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.HasOne(v => v.Product)
             .WithMany(p => p.Variants)
             .HasForeignKey(v => v.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(v => new { v.ProductId, v.Color, v.Size }).IsUnique();
        });

        // OrderDetail tham chiếu biến thể: KHÔNG cho xóa biến thể đã có đơn hàng
        modelBuilder.Entity<OrderDetail>()
            .HasOne(d => d.ProductVariant)
            .WithMany()
            .HasForeignKey(d => d.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cây danh mục cha - con: không cho xóa cha khi còn con
        modelBuilder.Entity<CategoryProduct>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Flash sale: xóa chương trình thì xóa luôn danh sách sản phẩm trong đó
        modelBuilder.Entity<FlashSaleItem>(e =>
        {
            e.HasOne(i => i.FlashSale)
             .WithMany(f => f.Items)
             .HasForeignKey(i => i.FlashSaleId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product)
             .WithMany()
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(i => new { i.FlashSaleId, i.ProductId }).IsUnique();
        });

        // Wishlist: 1 khách chỉ thích 1 sản phẩm 1 lần
        modelBuilder.Entity<WishlistItem>(e =>
        {
            e.HasIndex(w => new { w.CustomerId, w.ProductId }).IsUnique();
            e.HasOne(w => w.Customer)
             .WithMany()
             .HasForeignKey(w => w.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(w => w.Product)
             .WithMany()
             .HasForeignKey(w => w.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Giao dịch thanh toán: TxnRef DUY NHẤT — chốt chặn cuối cùng cho
        // webhook IPN idempotent (test case 11)
        modelBuilder.Entity<PaymentTransaction>(e =>
        {
            e.HasIndex(t => t.TxnRef).IsUnique();
            e.HasOne(t => t.Order)
             .WithMany()
             .HasForeignKey(t => t.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Refresh token: tra cứu nhanh theo chuỗi token
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(t => t.Token).IsUnique();
            e.HasOne(t => t.Customer)
             .WithMany()
             .HasForeignKey(t => t.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Lịch sử dùng mã: đếm nhanh "khách X đã dùng mã Y mấy lần" (test case 8)
        modelBuilder.Entity<DiscountUsage>(e =>
        {
            e.HasIndex(u => new { u.DiscountCodeId, u.CustomerEmail });
            e.HasOne(u => u.DiscountCode)
             .WithMany()
             .HasForeignKey(u => u.DiscountCodeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Mã giảm giá tra theo Code rất thường xuyên -> đánh index duy nhất
        modelBuilder.Entity<DiscountCode>()
            .HasIndex(d => d.Code)
            .IsUnique();

        // Index phục vụ BÁO CÁO & THỐNG KÊ (mục 6) — lọc đơn theo ngày/trạng thái
        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.OrderDate);
            e.HasIndex(o => o.Status);
        });

        // Thương hiệu: xóa Brand thì sản phẩm chỉ mất liên kết (SetNull)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
