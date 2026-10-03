using ERP.Domain.SavedViews;
using ERP.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class SavedViewConfiguration : IEntityTypeConfiguration<SavedView>
    {
        public void Configure(EntityTypeBuilder<SavedView> builder)
        {
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id)
                .IsRequired();

            builder.Property(v => v.UserId)
                .IsRequired();

            builder.Property(v => v.Screen)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(SavedView.NameMaxLength);

            builder.Property(v => v.Filters)
                .IsRequired()
                .HasMaxLength(SavedView.FiltersMaxLength);

            builder.Property(v => v.IsDefault)
                .IsRequired();

            builder.Property(v => v.CreatedAt)
                .IsRequired();

            builder.Property(v => v.CreatedBy)
                .IsRequired();

            builder.Property(v => v.UpdatedAt);

            builder.Property(v => v.UpdatedBy);

            // Versión (xmin): marcar una vista como predeterminada desmarca la anterior. Si dos pestañas lo hacen a la vez,
            // las dos desmarcan la misma vista y la segunda choca con la versión (409), en vez de dejar dos predeterminadas.
            builder.Property<uint>("RowVersion")
                .IsRowVersion();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // El nombre no se repite por usuario y pantalla (la aplicación además lo compara sin mayúsculas).
            builder.HasIndex(v => new { v.UserId, v.Screen, v.Name })
                .IsUnique();
        }
    }
}
