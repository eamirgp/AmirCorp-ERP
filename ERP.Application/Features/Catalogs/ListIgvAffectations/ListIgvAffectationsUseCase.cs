using ERP.Domain.Catalogs;

namespace ERP.Application.Features.Catalogs.ListIgvAffectations
{
    internal sealed class ListIgvAffectationsUseCase : IListIgvAffectationsUseCase
    {
        private static readonly IReadOnlyCollection<ListIgvAffectationsResponseDto> _igvAffectations =
            Enum.GetValues<IgvAffectation>()
                .Select(ia => new ListIgvAffectationsResponseDto(ia, ia.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListIgvAffectationsResponseDto>> ExecuteAsync() =>
            Task.FromResult(_igvAffectations);
    }
}
