using ERP.Application.Common.Pagination;
using ERP.Application.Contracts.Persistence.Queries;
using ERP.Application.Features.Audit;
using ERP.Application.Features.Audit.ListAuditEntries;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Queries
{
    internal sealed class AuditQueries : IAuditQueries
    {
        // Nombre del autor de los cambios que hace el sistema sin un usuario (ej.: el SuperAdmin al arrancar).
        private const string SystemUserName = "Sistema";

        private readonly ErpDbContext _context;

        public AuditQueries(ErpDbContext context) => _context = context;

        public async Task<PagedResult<AuditEntryDto>> ListAsync(AuditLogFilter filter)
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (filter.EntityType is not null)
                query = query.Where(a => a.EntityType == filter.EntityType);

            if (filter.EntityId is not null)
                query = query.Where(a => a.EntityId == filter.EntityId);

            if (filter.UserId is not null)
                query = query.Where(a => a.UserId == filter.UserId);

            if (filter.Action is not null)
                query = query.Where(a => a.Action == filter.Action);

            if (filter.FromUtc is not null)
                query = query.Where(a => a.OccurredAt >= filter.FromUtc);

            if (filter.ToUtcExclusive is not null)
                query = query.Where(a => a.OccurredAt < filter.ToUtcExclusive);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(a => a.EntityLabel.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();

            var rows = await query
                .OrderByDescending(a => a.OccurredAt)
                .ThenByDescending(a => a.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(a => new
                {
                    a.Id,
                    a.OccurredAt,
                    a.UserId,
                    UserName = _context.Users.Where(u => u.Id == a.UserId).Select(u => u.Name).FirstOrDefault(),
                    a.EntityType,
                    a.EntityId,
                    a.EntityLabel,
                    a.Action,
                    a.Changes
                })
                .ToArrayAsync();

            var items = rows
                .Select(a => new AuditEntryDto(
                    a.Id,
                    a.OccurredAt,
                    a.UserId,
                    a.UserName ?? SystemUserName,
                    a.EntityType,
                    a.EntityId,
                    a.EntityLabel,
                    a.Action,
                    a.Changes.Select(c => new AuditChangeDto(c.Field, c.From, c.To)).ToArray()
                    ))
                .ToArray();

            return new PagedResult<AuditEntryDto>(items, filter.Page, filter.PageSize, totalCount);
        }
    }
}
