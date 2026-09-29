using ERP.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .IsRequired();

            builder.Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(Product.CodeMaxLength);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(Product.NameMaxLength);

            builder.Property(p => p.UnitOfMeasure)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(3);

            builder.Property(p => p.IgvAffectation)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(p => p.SalePrice)
                .IsRequired()
                .HasPrecision(18, 6);

            builder.Property(p => p.IsActive)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .IsRequired();

            builder.Property(p => p.UpdatedAt);

            builder.Property(p => p.UpdatedBy);

            // Control de concurrencia optimista: en PostgreSQL se mapea a la columna de sistema xmin, que cambia en cada
            // actualización de la fila. El formulario envía la versión que vio y así no pisa cambios de otra persona.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.HasIndex(p => p.Code)
                .IsUnique();
        }
    }
}
