using ERP.Application.Contracts.Api;
using ERP.Application.Features.Audit;
using ERP.Domain.Common;
using ERP.Domain.Partners;
using ERP.Domain.Products;
using ERP.Domain.UnitsOfMeasure;
using ERP.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ERP.Persistence.Interceptors
{
    /// <summary>
    /// Antes de guardar: completa quién creó o modificó cada registro y escribe su historial en AuditLogs.
    /// Todo va en la misma transacción que el cambio, así ningún cambio queda sin registrar, venga de donde venga
    /// (pantallas, carga masiva o el propio sistema).
    /// </summary>
    internal sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUser _currentUser;

        public AuditInterceptor(ICurrentUser currentUser) => _currentUser = currentUser;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken ct = default)
        {
            if(eventData.Context is { } context)
            {
                var now = DateTime.UtcNow;
                var userId = _currentUser.IsAuthenticated ? _currentUser.Id : AuditableEntity.SystemUserId;
                var logs = new List<AuditLog>();

                // Los códigos de proveedores son parte del producto: su cambio cuenta como modificación del producto.
                var supplierCodeChanges = await SupplierCodeChangesAsync(context, ct);
                foreach (var productEntry in context.ChangeTracker.Entries<Product>())
                    if (productEntry.State == EntityState.Unchanged && supplierCodeChanges.ContainsKey(productEntry.Entity.Id))
                        productEntry.Property(p => p.UpdatedAt).IsModified = true;

                var unitNames = await UnitNamesAsync(context, ct);

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

                    var extraChanges = supplierCodeChanges.GetValueOrDefault(entry.Entity.Id) ?? [];
                    if (ToAuditLog(entry, now, userId, extraChanges, unitNames) is { } log)
                        logs.Add(log);
                }

                context.AddRange(logs);
            }

            return await base.SavingChangesAsync(eventData, result, ct);
        }

        private static AuditLog? ToAuditLog(
            EntityEntry<AuditableEntity> entry,
            DateTime now,
            Guid userId,
            IReadOnlyCollection<AuditChangeDto> extraChanges,
            IReadOnlyDictionary<string, string> unitNames)
        {
            if (AuditDescriber.Describe(entry.Entity) is not { } subject)
                return null;

            if (entry.State == EntityState.Added)
                return new AuditLog(now, userId, subject.Type, entry.Entity.Id, subject.Label, AuditAction.Created, []);

            if (entry.State != EntityState.Modified)
                return null;

            // Solo cuentan los campos que el usuario cambia; UpdatedAt, UpdatedBy y la versión quedan fuera.
            var modified = entry.Properties
                .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                .Where(p => p.Metadata.Name == AuditDescriber.PasswordProperty || AuditDescriber.IsAudited(subject.Type, p.Metadata.Name))
                .ToDictionary(p => p.Metadata.Name);

            if (modified.Count == 0 && extraChanges.Count == 0)
                return null;

            // La contraseña es una acción propia y nunca se guarda su valor.
            if (modified.ContainsKey(AuditDescriber.PasswordProperty))
                return new AuditLog(now, userId, subject.Type, entry.Entity.Id, subject.Label, AuditAction.PasswordReset, []);

            var action = extraChanges.Count > 0 ? AuditAction.Updated : ActionFor(modified);

            // En activar, desactivar y anular, la acción ya dice el cambio de estado; el resto de campos
            // (como el motivo de anulación) sí se muestran.
            var changes = modified.Values
                .Where(p => action == AuditAction.Updated || !AuditDescriber.IsStatus(p.Metadata.Name))
                .Select(p => p.Metadata.Name == AuditDescriber.UnitOfMeasureCodeProperty
                    ? AuditDescriber.Change(subject.Type, p.Metadata.Name, UnitName(p.OriginalValue, unitNames), UnitName(p.CurrentValue, unitNames))
                    : AuditDescriber.Change(subject.Type, p.Metadata.Name, p.OriginalValue, p.CurrentValue))
                .Concat(extraChanges)
                .Select(c => new AuditLogChange(c.Field, c.From, c.To))
                .ToList();

            return new AuditLog(now, userId, subject.Type, entry.Entity.Id, subject.Label, action, changes);
        }

        /// <summary>
        /// Códigos de proveedores agregados, cambiados o quitados, agrupados por producto y con el nombre del
        /// proveedor. Los de un producto nuevo no se listan: el historial solo dice que se creó.
        /// </summary>
        private static async Task<Dictionary<Guid, IReadOnlyCollection<AuditChangeDto>>> SupplierCodeChangesAsync(DbContext context, CancellationToken ct)
        {
            var newProducts = context.ChangeTracker.Entries<Product>()
                .Where(e => e.State == EntityState.Added)
                .Select(e => e.Entity.Id)
                .ToHashSet();

            var entries = context.ChangeTracker.Entries<ProductSupplierCode>()
                .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
                .Where(e => !newProducts.Contains(e.Entity.ProductId))
                .ToList();

            if (entries.Count == 0)
                return [];

            var supplierIds = entries.Select(e => e.Entity.SupplierId).Distinct().ToArray();
            var names = await context.Set<BusinessPartner>()
                .AsNoTracking()
                .Where(bp => supplierIds.Contains(bp.Id))
                .ToDictionaryAsync(bp => bp.Id, bp => bp.Name, ct);

            return entries
                .GroupBy(e => e.Entity.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyCollection<AuditChangeDto>)g
                        .Select(e =>
                        {
                            var code = e.Property(c => c.Code);
                            var name = names.GetValueOrDefault(e.Entity.SupplierId, "proveedor");
                            return e.State switch
                            {
                                EntityState.Added => AuditDescriber.SupplierCodeChange(name, null, code.CurrentValue),
                                EntityState.Deleted => AuditDescriber.SupplierCodeChange(name, code.OriginalValue, null),
                                _ => AuditDescriber.SupplierCodeChange(name, code.OriginalValue, code.CurrentValue),
                            };
                        })
                        .Where(c => c.From != c.To)
                        .OrderBy(c => c.Field)
                        .ToList());
        }

        /// <summary>
        /// Nombres de las unidades de medida, solo si algún producto cambió de unidad (el historial muestra
        /// "Unidad → Docena", no "NIU → DZN"). Son pocas: se leen todas.
        /// </summary>
        private static async Task<IReadOnlyDictionary<string, string>> UnitNamesAsync(DbContext context, CancellationToken ct)
        {
            var changed = context.ChangeTracker.Entries<Product>()
                .Any(e => e.State == EntityState.Modified && e.Property(p => p.UnitOfMeasureCode).IsModified);

            if (!changed)
                return new Dictionary<string, string>();

            return await context.Set<UnitOfMeasure>()
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Code, u => u.Name, ct);
        }

        private static object? UnitName(object? code, IReadOnlyDictionary<string, string> unitNames) =>
            code is string c && unitNames.TryGetValue(c, out var name) ? name : code;

        private static AuditAction ActionFor(IReadOnlyDictionary<string, PropertyEntry> modified)
        {
            if (modified.TryGetValue(AuditDescriber.IsCancelledProperty, out var cancelled) && cancelled.CurrentValue is true)
                return AuditAction.Cancelled;

            if (modified.Count == 1 && modified.TryGetValue(AuditDescriber.IsActiveProperty, out var active))
                return active.CurrentValue is true ? AuditAction.Activated : AuditAction.Deactivated;

            return AuditAction.Updated;
        }
    }
}
