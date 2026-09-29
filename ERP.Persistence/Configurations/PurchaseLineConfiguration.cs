using ERP.Domain.Products;
using ERP.Domain.Purchases;
using ERP.Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class PurchaseLineConfiguration : IEntityTypeConfiguration<PurchaseLine>
    {
        public void Configure(EntityTypeBuilder<PurchaseLine> builder)
        {
            builder.HasKey(pl => pl.Id);

            builder.Property(pl => pl.Id)
                .IsRequired();

            builder.Property(pl => pl.PurchaseId)
                .IsRequired();

            builder.Property(pl => pl.LineNumber)
                .IsRequired();

            builder.Property(pl => pl.ProductId)
                .IsRequired();

            builder.Property(pl => pl.ProductCode)
                .IsRequired()
                .HasMaxLength(Product.CodeMaxLength);

            builder.Property(pl => pl.ProductName)
                .IsRequired()
                .HasMaxLength(Product.NameMaxLength);

            builder.Property(pl => pl.InvoicePriceType)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(pl => pl.InvoiceIgvAffectation)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(pl => pl.InvoiceUnitOfMeasureCode)
                .IsRequired()
                .HasMaxLength(UnitOfMeasure.CodeMaxLength);

            builder.HasOne<UnitOfMeasure>()
                .WithMany()
                .HasForeignKey(pl => pl.InvoiceUnitOfMeasureCode)
                .HasPrincipalKey(u => u.Code)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(pl => pl.InvoiceQuantity)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(pl => pl.InvoiceUnitValue)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(pl => pl.InvoiceUnitPrice)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(pl => pl.ConversionFactor)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(pl => pl.InventoryQuantity)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(pl => pl.InventoryUnitCost)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(pl => pl.BaseAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(pl => pl.IgvAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(pl => pl.Total)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(pl => pl.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pl => new { pl.PurchaseId, pl.ProductId })
                .IsUnique();

            builder.HasIndex(pl => new { pl.PurchaseId, pl.LineNumber })
                .IsUnique();
        }
    }
}
