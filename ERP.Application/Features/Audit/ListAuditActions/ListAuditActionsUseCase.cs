namespace ERP.Application.Features.Audit.ListAuditActions
{
    internal sealed class ListAuditActionsUseCase : IListAuditActionsUseCase
    {
        private static readonly IReadOnlyCollection<ListAuditActionsResponseDto> _actions =
            Enum.GetValues<AuditAction>()
                .Select(a => new ListAuditActionsResponseDto(a, a.Description))
                .ToArray();

        public Task<IReadOnlyCollection<ListAuditActionsResponseDto>> ExecuteAsync() =>
            Task.FromResult(_actions);
    }
}
