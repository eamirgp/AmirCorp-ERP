using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using ERP.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Commands
{
    internal sealed class BusinessPartnerRepository : IBusinessPartnerRepository
    {
        private readonly ErpDbContext _context;

        public BusinessPartnerRepository(ErpDbContext context) => _context = context;

        public void Add(BusinessPartner businessPartner) =>
            _context.BusinessPartners
            .Add(businessPartner);

        public async Task<bool> DocumentNumberExistsAsync(string documentNumber, IdentityDocumentType identityDocumentType, Guid? excludeId = null) =>
            await _context.BusinessPartners
            .Where(bp => excludeId == null || bp.Id != excludeId)
            .AnyAsync(bp => bp.IdentityDocumentType == identityDocumentType && bp.DocumentNumber == documentNumber);

        public async Task<BusinessPartner?> GetByIdAsync(Guid id) =>
            await _context.BusinessPartners
            .FindAsync(id);

        public async Task<IReadOnlyCollection<BusinessPartner>> GetByIdsAsync(IReadOnlyCollection<Guid> ids) =>
            await _context.BusinessPartners
            .Where(bp => ids.Contains(bp.Id))
            .ToListAsync();
    }
}
