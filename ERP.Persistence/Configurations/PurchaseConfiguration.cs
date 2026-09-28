using ERP.Domain.Companies;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using ERP.Domain.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .IsRequired();

            builder.Property(p => p.CompanyId)
                .IsRequired();

            builder.Property(p => p.TaxDocumentType)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(p => p.Serie)
                .IsRequired()
                .HasMaxLength(Purchase.SerieMaxLength);

            builder.Property(p => p.Number)
                .IsRequired()
                .HasMaxLength(Purchase.NumberMaxLength);

            builder.Property(p => p.IssueDate)
                .IsRequired();

            builder.Property(p => p.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasConversion<string>();

            builder.Property(p => p.ExchangeRate)
                .HasPrecision(18, 6);

            builder.Property(p => p.InvoicePriceType)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(p => p.SupplierId)
                .IsRequired();

            builder.Property(p => p.SupplierIdentityDocumentType)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(p => p.SupplierDocumentNumber)
                .IsRequired()
                .HasMaxLength(IdentityDocumentTypeExtensions.ForeignMaxLength);

            builder.Property(p => p.SupplierName)
                .IsRequired()
                .HasMaxLength(BusinessPartner.NameMaxLength);

            builder.Property(p => p.TotalBaseAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.TotalIgvAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.Total)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(p => p.IsCancelled)
                .IsRequired();

            builder.Property(p => p.CancellationReason)
                .HasMaxLength(Purchase.CancellationReasonMaxLength);

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .IsRequired();

            builder.Property(p => p.UpdatedAt);

            builder.Property(p => p.UpdatedBy);

            // En PostgreSQL se mapea a la columna de sistema xmin, que cambia en cada actualización de la fila.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.Navigation(p => p.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(p => p.Lines)
                .WithOne()
                .HasForeignKey(pl => pl.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Company>()
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BusinessPartner>()
                .WithMany()
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => new { p.CompanyId, p.TaxDocumentType, p.SupplierId, p.Serie, p.Number })
                .IsUnique()
                .HasFilter("\"IsCancelled\" = false");
        }
    }
}
