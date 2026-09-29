using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class BusinessPartnerConfiguration : IEntityTypeConfiguration<BusinessPartner>
    {
        public void Configure(EntityTypeBuilder<BusinessPartner> builder)
        {
            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.IdentityDocumentType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(bp => bp.DocumentNumber)
                .IsRequired()
                .HasMaxLength(IdentityDocumentTypeExtensions.ForeignMaxLength);

            builder.Property(bp => bp.Name)
                .IsRequired()
                .HasMaxLength(BusinessPartner.NameMaxLength);

            builder.Property(bp => bp.CountryCode)
                .IsRequired()
                .HasMaxLength(Countries.CodeLength);

            builder.Property(bp => bp.IsClient)
                .IsRequired();

            builder.Property(bp => bp.IsSupplier)
                .IsRequired();

            builder.Property(bp => bp.IsActive)
                .IsRequired();

            builder.Property(bp => bp.CreatedAt)
                .IsRequired();

            builder.Property(bp => bp.CreatedBy)
                .IsRequired();

            builder.Property(bp => bp.UpdatedAt);

            builder.Property(bp => bp.UpdatedBy);

            // Control de concurrencia optimista (xmin): el formulario envía la versión que vio y no pisa cambios de otra persona.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.HasIndex(bp => new { bp.DocumentNumber, bp.IdentityDocumentType })
                .IsUnique();
        }
    }
}
