using ERP.Api.Controllers.BusinessPartners.Requests;
using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Domain.Catalogs;
using ERP.Domain.Partners.Enums;
using ERP.Domain.Purchases;

namespace ERP.Api.Controllers.Purchases.Requests
{
    public sealed record CreatePurchaseRequest(
        Guid? CompanyId,
        TaxDocumentType? TaxDocumentType,
        string? Serie,
        string? Number,
        DateOnly? IssueDate,
        Currency? Currency,
        decimal? ExchangeRate,
        InvoicePriceType? InvoicePriceType,
        Guid? SupplierId,
        // Proveedor que todavía no existe (RUC y razón social): se registra junto con la compra. Va en vez de SupplierId.
        CreatePurchaseNewSupplierRequest? NewSupplier,
        IReadOnlyCollection<CreatePurchaseLineRequest>? Lines
        )
    {
        private static readonly TimeZoneInfo PeruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");

        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (CompanyId is null || CompanyId == Guid.Empty)
                errors.Add("La empresa es requerida.");

            if (TaxDocumentType is null)
                errors.Add("El tipo de documento es requerido.");

            if (TaxDocumentType is not null && !Enum.IsDefined(TaxDocumentType.Value))
                errors.Add("El tipo de documento es inválido.");

            if (string.IsNullOrWhiteSpace(Serie))
                errors.Add("La serie es requerida.");

            if (!string.IsNullOrWhiteSpace(Serie) && Serie.Length != Purchase.SerieMaxLength)
                errors.Add($"La serie debe tener exactamente {Purchase.SerieMaxLength} caracteres.");

            if (!string.IsNullOrWhiteSpace(Serie) && !Serie.All(char.IsLetterOrDigit))
                errors.Add("La serie debe contener solo letras y números.");

            // La serie debe corresponder al comprobante (factura F…, boleta B…): la misma regla del dominio.
            if (TaxDocumentType is { } type && Enum.IsDefined(type)
                && !string.IsNullOrWhiteSpace(Serie) && Serie.Length == Purchase.SerieMaxLength && Serie.All(char.IsLetterOrDigit)
                && Purchase.SerieError(type, Serie) is { } serieError)
                errors.Add(serieError);

            if (string.IsNullOrWhiteSpace(Number))
                errors.Add("El número es requerido.");

            if (!string.IsNullOrWhiteSpace(Number) && Number.Length > Purchase.NumberMaxLength)
                errors.Add($"El número no puede exceder los {Purchase.NumberMaxLength} dígitos.");

            if (!string.IsNullOrWhiteSpace(Number) && !Number.All(char.IsDigit))
                errors.Add("El número debe contener solo dígitos.");

            if (IssueDate is null)
                errors.Add("La fecha de emisión es requerida.");

            if (IssueDate is not null && IssueDate > DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PeruTimeZone)))
                errors.Add("La fecha de emisión no puede ser mayor a la fecha actual.");

            if (Currency is null)
                errors.Add("La moneda es requerida.");

            if (Currency is not null && !Enum.IsDefined(Currency.Value))
                errors.Add("La moneda es inválida.");

            if (Currency is not null && Currency != Domain.Catalogs.Currency.PEN && ExchangeRate is null)
                errors.Add("El tipo de cambio es requerido para moneda extranjera.");

            if (Currency is Domain.Catalogs.Currency.PEN && ExchangeRate is not null)
                errors.Add("El tipo de cambio no aplica para soles.");

            if (ExchangeRate is not null && ExchangeRate <= 0)
                errors.Add("El tipo de cambio debe ser mayor a cero.");

            if (InvoicePriceType is null)
                errors.Add("El tipo de precio es requerido.");

            if (InvoicePriceType is not null && !Enum.IsDefined(InvoicePriceType.Value))
                errors.Add("El tipo de precio es inválido.");

            // El proveedor: uno registrado o uno nuevo, no los dos. El nuevo se valida como en su propia pantalla.
            if (NewSupplier is not null && SupplierId is not null)
                errors.Add("Elige un proveedor registrado o indica uno nuevo, no los dos.");
            else if (NewSupplier is not null)
                errors.AddRange(BusinessPartnerRequestRules.Validate(IdentityDocumentType.Ruc, NewSupplier.Ruc, countryCode: null, NewSupplier.Name)
                    .Select(e => $"Proveedor nuevo: {e}"));
            else if (SupplierId is null || SupplierId == Guid.Empty)
                errors.Add("El proveedor es requerido.");

            if (Lines is null || Lines.Count == 0)
                errors.Add("La compra debe tener al menos una línea.");
            else
            {
                var lines = Lines.ToArray();
                for (var i = 0; i < lines.Length; i++)
                    errors.AddRange(lines[i].Validate(i + 1, InvoicePriceType));

                var duplicatedGroups = lines
                    .Select((line, index) => new { line.ProductId, LineNumber = index + 1 })
                    .Where(l => l.ProductId is not null)
                    .GroupBy(l => l.ProductId!.Value)
                    .Where(g => g.Count() > 1);

                foreach (var group in duplicatedGroups)
                    errors.Add($"El producto está duplicado en las líneas {string.Join(", ", group.Select(x => x.LineNumber))}.");
            }

            return errors;
        }

        public CreatePurchaseDto ToDto() =>
            new(
                CompanyId!.Value,
                TaxDocumentType!.Value,
                Serie!,
                Number!,
                IssueDate!.Value,
                Currency!.Value,
                ExchangeRate,
                InvoicePriceType!.Value,
                SupplierId,
                NewSupplier is null ? null : new CreatePurchaseNewSupplierDto(NewSupplier.Ruc!, NewSupplier.Name!),
                Lines!.Select(l => l.ToDto()).ToArray()
                );
    }

    public sealed record CreatePurchaseNewSupplierRequest(string? Ruc, string? Name);
}
