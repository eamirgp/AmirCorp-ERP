using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCurrencies
{
    internal sealed class ListCurrenciesUseCase : IListCurrenciesUseCase
    {
        private static readonly IReadOnlyCollection<ListCurrenciesResponseDto> _currencies =
            Enum.GetValues<Currency>()
                .Select(c => new ListCurrenciesResponseDto(c, c.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListCurrenciesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_currencies);
    }
}
