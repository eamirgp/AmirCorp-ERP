using ERP.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations
{
    internal sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
    {
        public void Configure(EntityTypeBuilder<ExchangeRate> builder)
        {
            builder.ToTable("ExchangeRates");

            // Un tipo de cambio por moneda y fecha.
            builder.HasKey(e => new { e.Currency, e.Date });

            builder.Property(e => e.Currency)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(3);

            builder.Property(e => e.Date)
                .IsRequired();

            builder.Property(e => e.PublishedDate)
                .IsRequired();

            builder.Property(e => e.BuyRate)
                .HasPrecision(18, 6);

            builder.Property(e => e.SellRate)
                .HasPrecision(18, 6);

            builder.Property(e => e.Source)
                .IsRequired()
                .HasMaxLength(ExchangeRate.SourceMaxLength);

            builder.Property(e => e.FetchedAt)
                .IsRequired();
        }
    }
}
