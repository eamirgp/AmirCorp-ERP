using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    public sealed record GetPurchaseLineResponseDto(
        int LineNumber,
        Guid ProductId,
        string ProductCode,
        string ProductName,
        IgvAffectation InvoiceIgvAffectation,
        UnitOfMeasure InvoiceUnitOfMeasure,
        decimal InvoiceQuantity,
        decimal InvoiceUnitAmount,
        decimal BaseAmount,
        decimal IgvAmount,
        decimal Total
        )
    {
        public string InvoiceIgvAffectationDescription => InvoiceIgvAffectation.Description;
        public string InvoiceUnitOfMeasureDescription => InvoiceUnitOfMeasure.Description;
    }
}
