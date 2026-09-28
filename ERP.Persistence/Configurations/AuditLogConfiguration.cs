using System.Text.Json;
using ERP.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.OccurredAt)
                .IsRequired();

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.EntityType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(a => a.EntityId)
                .IsRequired();

            builder.Property(a => a.EntityLabel)
                .IsRequired()
                .HasMaxLength(AuditLog.EntityLabelMaxLength);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(a => a.Changes)
                .IsRequired()
                .HasColumnType("jsonb")
                .HasConversion(
                    changes => JsonSerializer.Serialize(changes, JsonSerializerOptions.Default),
                    json => JsonSerializer.Deserialize<List<AuditLogChange>>(json, JsonSerializerOptions.Default) ?? new List<AuditLogChange>(),
                    new ValueComparer<List<AuditLogChange>>(
                        (a, b) => a!.SequenceEqual(b!),
                        changes => changes.Aggregate(0, (hash, c) => HashCode.Combine(hash, c.GetHashCode())),
                        changes => changes.ToList()
                        )
                    );

            // El historial de un registro se pide por tipo e ID; la pantalla de auditoría, por fecha y por usuario.
            builder.HasIndex(a => new { a.EntityType, a.EntityId, a.OccurredAt });
            builder.HasIndex(a => a.OccurredAt);
            builder.HasIndex(a => a.UserId);
        }
    }
}
