using Microsoft.EntityFrameworkCore;
using Task_Interview.Models;

namespace Task_Interview.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
       
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<ArchivedTransaction> ArchivedTransactions { get; set; }
        public DbSet<ProductWarehouse> ProductWarehouses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<ProductWarehouse>()
               .HasKey(pw => new { pw.ProductId, pw.WarehouseId }); 

            modelBuilder.Entity<ProductWarehouse>()
                .HasOne(pw => pw.Product)
                .WithMany(p => p.ProductWarehouses)
                .HasForeignKey(pw => pw.ProductId);

            modelBuilder.Entity<ProductWarehouse>()
                .HasOne(pw => pw.Warehouse)
                .WithMany(w => w.ProductWarehouses)
                .HasForeignKey(pw => pw.WarehouseId);


            //indexing
            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => t.ProductId) 
                .HasDatabaseName("IX_InventoryTransaction_ProductId");

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => t.TransactionType)
                .HasDatabaseName("IX_InventoryTransaction_TransactionType");

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => t.Date)
                .HasDatabaseName("IX_InventoryTransaction_Date");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Category) 
                .HasDatabaseName("IX_Product_Category");

            modelBuilder.Entity<InventoryTransaction>()
               .HasIndex(t => new { t.ProductId, t.Date })
        .       HasDatabaseName("IX_InventoryTransaction_ProductId_Date");

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => new { t.TransactionType, t.Date })
                .HasDatabaseName("IX_InventoryTransaction_TransactionType_Date");

            modelBuilder.Entity<InventoryTransaction>()
                .HasIndex(t => new { t.ProductId, t.TransactionType })
                .HasDatabaseName("IX_InventoryTransaction_ProductId_TransactionType");

            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<InventoryTransaction>()
                                      .HasQueryFilter(it => !it.product.IsDeleted);

            modelBuilder.Entity<ProductWarehouse>()
                                      .HasQueryFilter(pw => !pw.Product.IsDeleted);
            
        }
    }
}
