using ERP.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(RefreshToken.HashLength);

            builder.Property(t => t.FamilyId)
                .IsRequired();

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            builder.Property(t => t.ExpiresAt)
                .IsRequired();

            builder.Property(t => t.SessionExpiresAt)
                .IsRequired();

            builder.Property(t => t.RevokedAt);

            builder.Property(t => t.ReplacedById);

            builder.Ignore(t => t.IsRevoked);

            // Dos renovaciones al mismo tiempo con el mismo token no pueden reemplazarlo las dos (quedarían dos sesiones
            // abiertas): la segunda choca con la versión (xmin) y se responde 409 sin guardar nada.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                // Como todas las llaves foráneas (decisión 4): los usuarios no se borran, se desactivan.
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.TokenHash)
                .IsUnique();

            builder.HasIndex(t => t.UserId);

            builder.HasIndex(t => t.FamilyId);
        }
    }
}
