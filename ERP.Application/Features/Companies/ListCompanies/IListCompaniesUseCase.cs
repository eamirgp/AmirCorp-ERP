using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Companies.ListCompanies
{
    public interface IListCompaniesUseCase : IQueryUseCase<IReadOnlyCollection<ListCompaniesResponseDto>> { }
}
