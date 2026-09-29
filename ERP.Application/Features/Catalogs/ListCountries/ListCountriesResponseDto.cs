namespace ERP.Application.Features.Catalogs.ListCountries
{
    /// <summary>País del catálogo N.° 04 de SUNAT: código ISO (CN, US…) y nombre en español.</summary>
    public sealed record ListCountriesResponseDto(
        string Code,
        string Name
        );
}
