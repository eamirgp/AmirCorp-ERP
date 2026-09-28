using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListCurrencies
{
    public sealed record ListCurrenciesResponseDto(
        Currency Currency,
        string Description
        );
}
