namespace ERP.Application.Features.Audit.ListAuditEntityTypes
{
    internal sealed class ListAuditEntityTypesUseCase : IListAuditEntityTypesUseCase
    {
        private static readonly IReadOnlyCollection<ListAuditEntityTypesResponseDto> _entityTypes =
            Enum.GetValues<AuditEntityType>()
                .Select(t => new ListAuditEntityTypesResponseDto(t, t.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListAuditEntityTypesResponseDto>> ExecuteAsync() =>
            Task.FromResult(_entityTypes);
    }
}
