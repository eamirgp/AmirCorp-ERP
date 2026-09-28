using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Companies.GetCompany
{
    internal sealed class GetCompanyUseCase : IGetCompanyUseCase
    {
        private readonly ICompanyQueries _companyQueries;

        public GetCompanyUseCase(ICompanyQueries companyQueries) => _companyQueries = companyQueries;

        public async Task<GetCompanyResponseDto?> ExecuteAsync(GetCompanyDto request) =>
            await _companyQueries.GetCompanyAsync(request);
    }
}
