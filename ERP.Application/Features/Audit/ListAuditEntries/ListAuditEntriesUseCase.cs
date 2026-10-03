using ERP.Application.Common.Pagination;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Audit.ListAuditEntries
{
    internal sealed class ListAuditEntriesUseCase : IListAuditEntriesUseCase
    {
        private static readonly TimeZoneInfo PeruTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Lima");
        private static readonly DateOnly MinDay = new(2000, 1, 1);
        private static readonly DateOnly MaxDay = new(2100, 12, 31);

        private readonly IAuditQueries _auditQueries;

        public ListAuditEntriesUseCase(IAuditQueries auditQueries) => _auditQueries = auditQueries;

        public async Task<Result<PagedResult<AuditEntryDto>>> ExecuteAsync(ListAuditEntriesDto request)
        {
            // Una fecha como 31/12/9999 no se puede convertir a hora UTC: se avisa en vez de fallar.
            if (request.From is { } outFrom && (outFrom < MinDay || outFrom > MaxDay)
                || request.To is { } outTo && (outTo < MinDay || outTo > MaxDay))
                return Result<PagedResult<AuditEntryDto>>.Failure(
                    [$"Revisa el año de las fechas: debe estar entre {MinDay.Year} y {MaxDay.Year}."], ErrorType.BadRequest);

            if (request.From is { } from && request.To is { } to && from > to)
                return Result<PagedResult<AuditEntryDto>>.Failure(["La fecha inicial no puede ser posterior a la fecha final."], ErrorType.BadRequest);

            var filter = new AuditLogFilter(
                request.Page,
                request.PageSize,
                request.EntityType,
                request.EntityId,
                request.UserId,
                request.Action,
                request.From is { } start ? StartOfDayUtc(start) : null,
                request.To is { } end ? StartOfDayUtc(end.AddDays(1)) : null,
                request.SearchTerm
                );

            return Result<PagedResult<AuditEntryDto>>.Success(await _auditQueries.ListAsync(filter));
        }

        // Un día en Perú empieza a las 00:00 de Lima, que en UTC son las 05:00.
        private static DateTime StartOfDayUtc(DateOnly day) =>
            TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), PeruTimeZone);
    }
}
