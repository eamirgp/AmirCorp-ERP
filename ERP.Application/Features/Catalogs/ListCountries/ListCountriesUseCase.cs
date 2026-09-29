using System.Globalization;
using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCountries
{
    /// <summary>
    /// Países para elegir con un documento extranjero, ordenados por nombre. Perú no está: con DNI o RUC el país ya es
    /// Perú y no se pregunta, y un documento extranjero no puede ser de Perú.
    /// </summary>
    internal sealed class ListCountriesUseCase : IListCountriesUseCase
    {
        private static readonly IReadOnlyCollection<ListCountriesResponseDto> _countries =
            Countries.All
                .Where(c => c.Key != Countries.Peru)
                .OrderBy(c => c.Value, StringComparer.Create(CultureInfo.GetCultureInfo("es-PE"), ignoreCase: true))
                .Select(c => new ListCountriesResponseDto(c.Key, c.Value))
                .ToArray();

        public Task<IReadOnlyCollection<ListCountriesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_countries);
    }
}
