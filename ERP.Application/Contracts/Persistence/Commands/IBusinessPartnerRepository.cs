using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IBusinessPartnerRepository
    {
        void Add(BusinessPartner businessPartner);
        Task<bool> DocumentNumberExistsAsync(string documentNumber, IdentityDocumentType identityDocumentType, Guid? excludeId = null);
        Task<BusinessPartner?> GetByIdAsync(Guid id);
    }
}
