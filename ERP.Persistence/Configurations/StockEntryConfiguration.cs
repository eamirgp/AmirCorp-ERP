using ERP.Domain.Companies;
using ERP.Domain.Inventory;
using ERP.Domain.Products;
using ERP.Domain.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
    {
        public void Configure(EntityTypeBuilder<StockEntry> builder)
        {
            builder.HasKey(se => se.Id);

            builder.Property(se => se.Id)
                .IsRequired();

            builder.Property(se => se.CompanyId)
                .IsRequired();

            builder.Property(se => se.ProductId)
                .IsRequired();

            builder.Property(se => se.PurchaseLineId)
                .IsRequired();

            builder.Property(se => se.OriginalQuantity)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(se => se.RemainingQuantity)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(se => se.UnitCost)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(se => se.EntryDate)
                .IsRequired();

            // En PostgreSQL se mapea a la columna de sistema xmin, que cambia en cada actualización de la fila.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.HasOne<Company>()
                .WithMany()
                .HasForeignKey(se => se.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(se => se.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PurchaseLine>()
                .WithMany()
                .HasForeignKey(se => se.PurchaseLineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
