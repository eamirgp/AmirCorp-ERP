using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCountries
{
    public sealed record ListCountriesResponseDto(
        Country Country,
        string Name
        );
}
