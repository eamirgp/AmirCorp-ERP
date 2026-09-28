using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCountries
{
    internal sealed class ListCountriesUseCase : IListCountriesUseCase
    {
        private static readonly IReadOnlyCollection<ListCountriesResponseDto> _countries =
            Enum.GetValues<Country>()
                .Select(c => new ListCountriesResponseDto(c, c.Name))
                .ToArray();

        public Task<IReadOnlyCollection<ListCountriesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_countries);
    }
}
