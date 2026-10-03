using ERP.Domain.Catalogs;
using ERP.Domain.Companies;
using ERP.Persistence.Auditing;
using ERP.Domain.Inventory;
using ERP.Domain.Partners;
using ERP.Domain.Products;
using ERP.Domain.Purchases;
using ERP.Domain.SavedViews;
using ERP.Domain.UnitsOfMeasure;
using ERP.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Context
{
    internal sealed class ErpDbContext : DbContext
    {
        public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // unaccent: las búsquedas por nombre ignoran las tildes ("camara" encuentra "Cámara").
            modelBuilder.HasPostgresExtension("unaccent");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductSupplierCode> ProductSupplierCodes => Set<ProductSupplierCode>();
        public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
        public DbSet<BusinessPartner> BusinessPartners => Set<BusinessPartner>();
        public DbSet<StockEntry> StockEntries => Set<StockEntry>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<SavedView> SavedViews => Set<SavedView>();
        public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    }
}
