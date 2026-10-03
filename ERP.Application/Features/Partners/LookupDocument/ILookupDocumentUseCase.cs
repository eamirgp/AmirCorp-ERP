using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.LookupDocument
{
    public interface ILookupDocumentUseCase
    {
        /// <param name="partnerId">El registro que se está editando, si lo hay: tener ese documento él mismo no es un duplicado.</param>
        Task<Result<LookupDocumentResponseDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber, Guid? partnerId = null, CancellationToken ct = default);
    }
}
