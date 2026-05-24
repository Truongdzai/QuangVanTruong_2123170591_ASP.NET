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
    }
}
