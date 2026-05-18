using System.Data.Entity;

namespace ConfectioneryShop.Data.Ef
{
    /// <summary>Контекст EF Code First для той же БД, что и ADO.NET (лаб. 9).</summary>
    public class ConfectioneryDbContext : DbContext
    {
        public ConfectioneryDbContext()
            : base("name=Confectionery")
        {
            Database.SetInitializer<ConfectioneryDbContext>(null);
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<ManufacturerEntity> Manufacturers { get; set; }
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<TagEntity> Tags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CategoryEntity>()
                .HasMany(c => c.Products)
                .WithRequired(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<ManufacturerEntity>()
                .HasMany(m => m.Products)
                .WithRequired(p => p.Manufacturer)
                .HasForeignKey(p => p.ManufacturerId);

            modelBuilder.Entity<ProductEntity>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .Map(m =>
                {
                    m.ToTable("ProductTag");
                    m.MapLeftKey("ProductId");
                    m.MapRightKey("TagId");
                });
        }
    }
}
