using ERP.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .IsRequired();

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(User.NameMaxLength);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(User.EmailMaxLength);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.CreatedBy)
                .IsRequired();

            builder.Property(u => u.UpdatedAt);

            builder.Property(u => u.UpdatedBy);

            // En PostgreSQL se mapea a la columna de sistema xmin, que cambia en cada actualización de la fila.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.HasIndex(u => u.Role)
                .IsUnique()
                .HasFilter("\"Role\" = 'SuperAdmin'");

            builder.HasIndex(u => u.Name)
                .IncludeProperties(u => new { u.Email, u.Role, u.IsActive });
        }
    }
}
