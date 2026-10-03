using ERP.Api.Controllers.BusinessPartners.Requests;
using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Domain.Catalogs;
using ERP.Domain.Common;
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
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (CompanyId is null || CompanyId == Guid.Empty)
                errors.Add("La empresa es requerida.");

            // Las reglas del comprobante son las del dominio: aquí solo se juntan para avisar todo de una vez.
            var validType = Purchase.TaxDocumentTypeError(TaxDocumentType) is null ? TaxDocumentType : null;
            string?[] documentErrors =
            [
                Purchase.TaxDocumentTypeError(TaxDocumentType),
                Purchase.SerieError(validType, Serie),
                Purchase.NumberError(Number),
                Purchase.IssueDateError(IssueDate, PeruCalendar.Today(DateTime.UtcNow)),
                Purchase.CurrencyError(Currency),
                Purchase.ExchangeRateError(Currency, ExchangeRate),
                Purchase.InvoicePriceTypeError(InvoicePriceType),
            ];
            errors.AddRange(documentErrors.OfType<string>());

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
                Serie!.Trim(),
                Number!.Trim(),
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
