using ERP.Application.Common.Results;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.FindBusinessPartnerByDocument
{
    public interface IFindBusinessPartnerByDocumentUseCase
    {
        /// <returns>Quién tiene ese documento, o 404 si nadie (el documento está libre).</returns>
        Task<Result<FoundBusinessPartnerDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber);
    }
}
