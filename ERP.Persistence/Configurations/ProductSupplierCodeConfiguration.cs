using ERP.Domain.Partners;
using ERP.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class ProductSupplierCodeConfiguration : IEntityTypeConfiguration<ProductSupplierCode>
    {
        public void Configure(EntityTypeBuilder<ProductSupplierCode> builder)
        {
            builder.HasKey(c => c.Id);

            // El dominio crea el Id. Así EF inserta el código nuevo que se agrega a un producto ya guardado,
            // en vez de tomarlo por uno existente.
            builder.Property(c => c.Id)
                .IsRequired()
                .ValueGeneratedNever();

            builder.Property(c => c.ProductId)
                .IsRequired();

            builder.Property(c => c.SupplierId)
                .IsRequired();

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(ProductSupplierCode.CodeMaxLength);

            builder.HasOne<BusinessPartner>()
                .WithMany()
                .HasForeignKey(c => c.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Un código por proveedor en cada producto.
            builder.HasIndex(c => new { c.ProductId, c.SupplierId })
                .IsUnique();

            // Un código de un proveedor apunta a un solo producto: así su factura se puede relacionar sin dudas.
            builder.HasIndex(c => new { c.SupplierId, c.Code })
                .IsUnique();
        }
    }
}
