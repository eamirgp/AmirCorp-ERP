using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Companies.GetCompany
{
    internal sealed class GetCompanyUseCase : IGetCompanyUseCase
    {
        private readonly ICompanyQueries _companyQueries;

        public GetCompanyUseCase(ICompanyQueries companyQueries) => _companyQueries = companyQueries;

        public async Task<Result<GetCompanyResponseDto>> ExecuteAsync(GetCompanyDto request) =>
            Result<GetCompanyResponseDto>.FoundOr(await _companyQueries.GetCompanyAsync(request), "La empresa no existe.");
    }
}
