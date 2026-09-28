using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Catalogs.ListCountries
{
    public interface IListCountriesUseCase : IQueryUseCase<IReadOnlyCollection<ListCountriesResponseDto>> { }
}
