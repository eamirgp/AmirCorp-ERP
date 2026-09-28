using ERP.Application.Contracts.Api;
using ERP.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Persistence.Interceptors
{
    internal sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUser _currentUser;

        public AuditInterceptor(ICurrentUser currentUser) => _currentUser = currentUser;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            if(eventData.Context is { } context)
            {
                var now = DateTime.UtcNow;
                var userId = _currentUser.IsAuthenticated ? _currentUser.Id : AuditableEntity.SystemUserId;

                foreach(var entry in context.ChangeTracker.Entries<AuditableEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Property(e => e.CreatedAt).CurrentValue = now;
                        entry.Property(e => e.CreatedBy).CurrentValue = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entry.Property(e => e.UpdatedAt).CurrentValue = now;
                        entry.Property(e => e.UpdatedBy).CurrentValue = userId;
                    }
                }
            }

            return base.SavingChangesAsync(eventData, result, ct);
        }
    }
}
