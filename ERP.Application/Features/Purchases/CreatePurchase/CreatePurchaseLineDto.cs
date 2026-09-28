using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    public sealed record CreatePurchaseLineDto(
        Guid ProductId,
        IgvAffectation InvoiceIgvAffectation,
        UnitOfMeasure InvoiceUnitOfMeasure,
        decimal InvoiceQuantity,
        decimal InvoiceAmount,
        decimal ConversionFactor
        );
}
