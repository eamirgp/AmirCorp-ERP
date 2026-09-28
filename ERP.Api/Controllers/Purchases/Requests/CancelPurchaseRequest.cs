using ERP.Application.Features.Purchases.CancelPurchase;
using ERP.Domain.Purchases;

namespace ERP.Api.Controllers.Purchases.Requests
{
    public sealed record CancelPurchaseRequest(string? CancellationReason)
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(CancellationReason))
                errors.Add("El motivo de anulación es requerido.");
            
            if (CancellationReason is not null && CancellationReason.Length > Purchase.CancellationReasonMaxLength)
                errors.Add($"El motivo de anulación no puede exceder los {Purchase.CancellationReasonMaxLength} caracteres.");

            return errors;
        }

        public CancelPurchaseDto ToDto(Guid id) =>
            new(
                id,
                CancellationReason!
                );
    }
}