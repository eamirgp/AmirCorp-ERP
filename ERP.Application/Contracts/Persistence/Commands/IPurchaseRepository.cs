using ERP.Domain.Catalogs;
using ERP.Domain.Purchases;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IPurchaseRepository
    {
        void Add(Purchase purchase);

        /// <summary>
        /// La compra no anulada con ese comprobante del proveedor, en cualquier empresa, o null si no hay.
        /// La serie y el número se comparan normalizados ("f001 " es F001; "25" es 00000025).
        /// </summary>
        Task<RegisteredDocument?> FindDocumentAsync(TaxDocumentType taxDocumentType, Guid supplierId, string serie, string number);

        Task<Purchase?> GetByIdWithLinesAsync(Guid id);

        /// <summary>Si el proveedor tiene alguna compra, anuladas incluidas.</summary>
        Task<bool> ExistsBySupplierAsync(Guid supplierId);

        /// <summary>Si la empresa tiene alguna compra, anuladas incluidas.</summary>
        Task<bool> ExistsByCompanyAsync(Guid companyId);
    }

    /// <summary>Empresa en la que ya está registrado un comprobante (con su razón social copiada en la compra).</summary>
    public sealed record RegisteredDocument(Guid CompanyId, string CompanyName);
}
