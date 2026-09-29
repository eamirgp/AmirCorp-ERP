using ERP.Domain.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
    {
        public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
        {
            builder.ToTable("UnitsOfMeasure");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .IsRequired();

            builder.Property(u => u.Code)
                .IsRequired()
                .HasMaxLength(UnitOfMeasure.CodeMaxLength);

            builder.Property(u => u.SunatName)
                .IsRequired()
                .HasMaxLength(UnitOfMeasure.SunatNameMaxLength);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(UnitOfMeasure.NameMaxLength);

            builder.Property(u => u.FixedConversionFactor)
                .HasPrecision(18, 6);

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.CreatedBy)
                .IsRequired();

            // Productos y líneas de compra guardan el código SUNAT, no el Id: es el dato que va en la factura.
            builder.HasAlternateKey(u => u.Code);
        }
    }
}
