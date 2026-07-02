using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopApi;
namespace ShopApi
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users {get; set;}
        public DbSet<Product> Products {get; set;}
        public DbSet<Category> Categories {get; set;}
        public DbSet<Order> Orders {get; set;}
        public DbSet<OrderItem> OrderItems {get; set;}
        public DbSet<RefreshToken> RefreshTokens {get; set;}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.Property(u => u.FirstName)
                    .HasMaxLength(100)
                    .IsRequired(false);
                
                entity.Property(u => u.DeliveryAddress)
                    .HasMaxLength(250)
                    .IsRequired(false);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.user)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");

                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => new { rt.UserId, rt.Expires });

                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshToekens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Product>(entity =>
           {
               entity.ToTable("Products");

               entity.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(200);

               entity.Property(p => p.Description)
                   .HasMaxLength(2000);

               entity.Property(p => p.Price)
                   .HasPrecision(18, 2); // 18 цифр, 2 после запятой

               entity.Property(p => p.DiscountPrice)
                   .HasPrecision(18, 2);

               // Индексы для быстрого поиска
               entity.HasIndex(p => p.Name)
                   .HasDatabaseName("IX_Products_Name");

               entity.HasIndex(p => p.CategoryId)
                   .HasDatabaseName("IX_Products_CategoryId");

               entity.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict); // Не удалять категорию, если есть товары
           });

            // Настройка Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Description)
                    .HasMaxLength(500);

                entity.HasOne(c => c.ParentCategory)
                    .WithMany(c => c.ChildCategories)
                    .HasForeignKey(c => c.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Индексы
                entity.HasIndex(c => c.Name)
                    .HasDatabaseName("IX_Categories_Name");
            });

            // Настройка Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.Property(o => o.TotalAmount)
                    .HasPrecision(18, 2);

                entity.Property(o => o.Subtotal)
                    .HasPrecision(18, 2);

                entity.Property(o => o.DiscountAmount)
                    .HasPrecision(18, 2);

                entity.Property(o => o.PromoCodeDiscount)
                    .HasPrecision(18, 2);

                // Индексы
                entity.HasIndex(o => o.OrderId)
                    .IsUnique()
                    .HasDatabaseName("IX_Orders_OrderId");

                entity.HasIndex(o => o.UserId)
                    .HasDatabaseName("IX_Orders_UserId");

                entity.HasIndex(o => o.Status)
                    .HasDatabaseName("IX_Orders_Status");

                entity.HasIndex(o => o.CreatedAt)
                    .HasDatabaseName("IX_Orders_CreatedAt");

                entity.HasIndex(o => new { o.UserId, o.CreatedAt })
                    .HasDatabaseName("IX_Orders_User_CreatedAt");

                entity.HasIndex(o => new { o.Status, o.CreatedAt })
                    .HasDatabaseName("IX_Orders_Status_CreatedAt");

                // Связи
                entity.HasOne(o => o.user)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.SetNull); // При удалении пользователя, заказы остаются
            });

            // Настройка OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");

                entity.Property(oi => oi.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(oi => oi.UnitPrice)
                    .HasPrecision(18, 2);

                entity.Property(oi => oi.DiscountPrice)
                    .HasPrecision(18, 2);

                entity.Property(oi => oi.DiscountAmount)
                    .HasPrecision(18, 2);

                // Индексы
                entity.HasIndex(oi => oi.OrderId)
                    .HasDatabaseName("IX_OrderItems_OrderId");

                entity.HasIndex(oi => oi.ProductId)
                    .HasDatabaseName("IX_OrderItems_ProductId");

                entity.HasIndex(oi => new { oi.OrderId, oi.ProductId })
                    .HasDatabaseName("IX_OrderItems_Order_Product");

                // Связи
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });
        }
    }
} 