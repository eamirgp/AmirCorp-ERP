namespace ERP.Application.Features.Purchases.CancelPurchase
{
    public sealed record CancelPurchaseDto(
        Guid Id,
        string CancellationReason
        );
}
