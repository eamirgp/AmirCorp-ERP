using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Companies.LookupRuc
{
    public interface ILookupCompanyRucUseCase
    {
        /// <param name="companyId">La empresa que se está editando, si la hay: tener ese RUC ella misma no es un duplicado.</param>
        Task<Result<LookupDocumentResponseDto>> ExecuteAsync(string ruc, Guid? companyId = null, CancellationToken ct = default);
    }
}
