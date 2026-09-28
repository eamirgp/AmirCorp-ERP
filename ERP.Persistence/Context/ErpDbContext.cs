using ERP.Domain.Companies;
using ERP.Persistence.Auditing;
using ERP.Domain.Inventory;
using ERP.Domain.Partners;
using ERP.Domain.Products;
using ERP.Domain.Purchases;
using ERP.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Context
{
    internal sealed class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<BusinessPartner> BusinessPartners => Set<BusinessPartner>();
        public DbSet<StockEntry> StockEntries => Set<StockEntry>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    }
}
