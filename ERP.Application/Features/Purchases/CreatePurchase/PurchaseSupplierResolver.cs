using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    /// <summary>
    /// El proveedor de la compra: el elegido (<c>SupplierId</c>) o el que viene escrito en la compra (<c>NewSupplier</c>),
    /// que se registra con ella en la misma transacción.
    /// </summary>
    internal sealed class PurchaseSupplierResolver
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;

        public PurchaseSupplierResolver(IBusinessPartnerRepository businessPartnerRepository) =>
            _businessPartnerRepository = businessPartnerRepository;

        /// <returns>El proveedor, o null si el elegido no existe.</returns>
        public async Task<BusinessPartner?> ResolveAsync(CreatePurchaseDto request)
        {
            if (request.NewSupplier is not { } newSupplier)
                return await _businessPartnerRepository.GetByIdAsync(request.SupplierId!.Value);

            // Si entretanto alguien ya lo registró (o existía solo como cliente), se usa ese registro en vez de duplicarlo.
            var existing = await _businessPartnerRepository.FindByDocumentAsync(IdentityDocumentType.Ruc, newSupplier.Ruc);
            if (existing is null)
            {
                var supplier = BusinessPartner.Create(IdentityDocumentType.Ruc, newSupplier.Ruc, Countries.Peru, newSupplier.Name, isClient: false, isSupplier: true);
                _businessPartnerRepository.Add(supplier);
                return supplier;
            }

            var partner = await _businessPartnerRepository.GetByIdAsync(existing.Id);
            if (partner is { IsSupplier: false })
                partner.AddSupplierRole();

            return partner;
        }
    }
}
