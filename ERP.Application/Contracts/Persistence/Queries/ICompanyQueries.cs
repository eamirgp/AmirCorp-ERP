using ERP.Application.Common.Pagination;
using ERP.Application.Features.Companies.GetCompany;
using ERP.Application.Features.Companies.ListCompanies;

namespace ERP.Application.Contracts.Persistence.Queries
{
    public interface ICompanyQueries
    {
        Task<IReadOnlyCollection<ListCompaniesResponseDto>> ListCompaniesAsync(ListFilterDto filter);
        Task<GetCompanyResponseDto?> GetCompanyAsync(GetCompanyDto getCompanyDto);
    }
}
