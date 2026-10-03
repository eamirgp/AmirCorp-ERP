using ERP.Application.Common.Exceptions;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ERP.Persistence.Commands
{
    internal sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ErpDbContext _context;

        public UnitOfWork(ErpDbContext context) => _context = context;

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException(ex);
            }
            // Los casos de uso revisan que un código o documento no se repita, pero si dos personas guardan el mismo
            // a la vez, lo frena el índice único de la base. Se responde 409 en vez de un error del servidor; el texto
            // lo arma Application según la entidad de la tabla.
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
            {
                var entityType = _context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName() == pg.TableName)?.ClrType;
                throw ConcurrencyException.Duplicate(entityType, ex);
            }
        }
    }
}
