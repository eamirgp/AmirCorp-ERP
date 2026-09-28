using ERP.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .IsRequired();

            builder.Property(c => c.Ruc)
                .IsRequired()
                .HasMaxLength(Company.RucLength);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(Company.NameMaxLength);

            builder.Property(c => c.IsActive)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.CreatedBy)
                .IsRequired();

            builder.Property(c => c.UpdatedAt);

            builder.Property(c => c.UpdatedBy);

            builder.HasIndex(c => c.Ruc)
                .IsUnique();
        }
    }
}
