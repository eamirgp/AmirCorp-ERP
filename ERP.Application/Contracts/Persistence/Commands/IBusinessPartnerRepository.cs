using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IBusinessPartnerRepository
    {
        void Add(BusinessPartner businessPartner);

        /// <summary>Quién ya tiene ese documento (se compara normalizado), sin contar al que se edita.</summary>
        Task<BusinessPartner?> FindByDocumentAsync(IdentityDocumentType identityDocumentType, string documentNumber, Guid? excludeId = null);

        Task<BusinessPartner?> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<BusinessPartner>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);

        /// <summary>Versión actual del registro en la base (cambia con cada modificación).</summary>
        uint VersionOf(BusinessPartner businessPartner);
    }
}
