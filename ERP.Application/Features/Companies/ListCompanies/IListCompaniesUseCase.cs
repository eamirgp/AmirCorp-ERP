using ERP.Application.Common.Pagination;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Companies.ListCompanies
{
    public interface IListCompaniesUseCase : IQueryUseCase<ListFilterDto, IReadOnlyCollection<ListCompaniesResponseDto>> { }
}
