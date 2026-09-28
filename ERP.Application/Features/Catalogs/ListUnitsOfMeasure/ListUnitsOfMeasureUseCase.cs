using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListUnitsOfMeasure
{
    internal sealed class ListUnitsOfMeasureUseCase : IListUnitsOfMeasureUseCase
    {
        private static readonly IReadOnlyCollection<ListUnitsOfMeasureResponseDto> _unitsOfMeasure =
            Enum.GetValues<UnitOfMeasure>()
                .Select(um => new ListUnitsOfMeasureResponseDto(um, um.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListUnitsOfMeasureResponseDto>> ExecuteAsync() =>
            Task.FromResult(_unitsOfMeasure);
    }
}
