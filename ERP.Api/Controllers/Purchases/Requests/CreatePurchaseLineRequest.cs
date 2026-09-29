using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Domain.Catalogs;

namespace ERP.Api.Controllers.Purchases.Requests
{
    public sealed record CreatePurchaseLineRequest(
        Guid? ProductId,
        IgvAffectation? InvoiceIgvAffectation,
        string? InvoiceUnitOfMeasureCode,
        decimal? InvoiceQuantity,
        decimal? InvoiceAmount,
        decimal? ConversionFactor
        )
    {
        public IReadOnlyCollection<string> Validate(int lineNumber, InvoicePriceType? invoicePriceType)
        {
            var errors = new List<string>();

            var amountLabel = invoicePriceType is InvoicePriceType.UnitPrice
                ? "El precio unitario"
                : "El valor unitario";

            if (ProductId is null || ProductId == Guid.Empty)
                errors.Add($"Línea {lineNumber}: El producto es requerido.");

            if (InvoiceIgvAffectation is null)
                errors.Add($"Línea {lineNumber}: El tipo de afectación del IGV es requerido.");

            if (InvoiceIgvAffectation is not null && !Enum.IsDefined(InvoiceIgvAffectation.Value))
                errors.Add($"Línea {lineNumber}: El tipo de afectación del IGV es inválido.");

            if (string.IsNullOrWhiteSpace(InvoiceUnitOfMeasureCode))
                errors.Add($"Línea {lineNumber}: La unidad de medida es requerida.");

            if (InvoiceQuantity is null)
                errors.Add($"Línea {lineNumber}: La cantidad es requerida.");

            if (InvoiceQuantity is not null && InvoiceQuantity <= 0)
                errors.Add($"Línea {lineNumber}: La cantidad debe ser mayor a cero.");

            if (InvoiceAmount is null)
                errors.Add($"Línea {lineNumber}: {amountLabel} es requerido.");

            if (InvoiceAmount is not null && InvoiceAmount <= 0)
                errors.Add($"Línea {lineNumber}: {amountLabel} debe ser mayor a cero.");

            if (ConversionFactor is null)
                errors.Add($"Línea {lineNumber}: El factor de conversión es requerido.");

            if (ConversionFactor is not null && ConversionFactor <= 0)
                errors.Add($"Línea {lineNumber}: El factor de conversión debe ser mayor a cero.");

            return errors;
        }

        public CreatePurchaseLineDto ToDto() =>
            new(
                ProductId!.Value,
                InvoiceIgvAffectation!.Value,
                InvoiceUnitOfMeasureCode!,
                InvoiceQuantity!.Value,
                InvoiceAmount!.Value,
                ConversionFactor!.Value
                );
    }
}
