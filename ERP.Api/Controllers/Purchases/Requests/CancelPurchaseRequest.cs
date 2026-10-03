using ERP.Application.Features.Purchases.CancelPurchase;
using ERP.Domain.Purchases;

namespace ERP.Api.Controllers.Purchases.Requests
{
    public sealed record CancelPurchaseRequest(string? CancellationReason)
    {
        public IReadOnlyCollection<string> Validate() =>
            Purchase.CancellationReasonError(CancellationReason) is { } error ? [error] : [];

        public CancelPurchaseDto ToDto(Guid id) =>
            new(
                id,
                CancellationReason!
                );
    }
}