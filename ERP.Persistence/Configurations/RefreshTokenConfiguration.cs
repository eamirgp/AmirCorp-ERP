using ERP.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        // SHA-256 en hexadecimal: 64 caracteres.
        private const int HashLength = 64;

        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.TokenHash)
                .IsRequired()
                .HasMaxLength(HashLength);

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

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.TokenHash)
                .IsUnique();

            builder.HasIndex(t => t.UserId);

            builder.HasIndex(t => t.FamilyId);
        }
    }
}
