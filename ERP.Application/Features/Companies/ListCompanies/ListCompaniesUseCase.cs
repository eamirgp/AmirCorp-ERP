using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Companies.ListCompanies
{
    internal sealed class ListCompaniesUseCase : IListCompaniesUseCase
    {
        private readonly ICompanyQueries _companyQueries;

        public ListCompaniesUseCase(ICompanyQueries companyQueries) => _companyQueries = companyQueries;

        public async Task<IReadOnlyCollection<ListCompaniesResponseDto>> ExecuteAsync() =>
            await _companyQueries.ListCompaniesAsync();
    }
}
