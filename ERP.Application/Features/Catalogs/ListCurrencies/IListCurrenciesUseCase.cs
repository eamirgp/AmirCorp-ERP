using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Catalogs.ListCurrencies
{
    public interface IListCurrenciesUseCase : IQueryUseCase<IReadOnlyCollection<ListCurrenciesResponseDto>> { }
}
