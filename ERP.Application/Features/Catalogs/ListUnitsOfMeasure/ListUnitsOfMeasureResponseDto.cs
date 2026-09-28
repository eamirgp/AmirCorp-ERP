using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListUnitsOfMeasure
{
    public sealed record ListUnitsOfMeasureResponseDto(
        UnitOfMeasure UnitOfMeasure,
        string Description
        );
}
