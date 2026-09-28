using ERP.Application.Common.Exceptions;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

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
        }
    }
}
