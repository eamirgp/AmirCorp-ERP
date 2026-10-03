using ERP.Domain.Catalogs;
using ERP.Domain.Purchases;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IPurchaseRepository
    {
        void Add(Purchase purchase);
        Task<bool> DocumentExistsAsync(Guid companyId, TaxDocumentType taxDocumentType, Guid supplierId, string serie, string number);
        Task<Purchase?> GetByIdWithLinesAsync(Guid id);

        /// <summary>Cuántas compras tiene el proveedor, anuladas incluidas.</summary>
        Task<int> CountBySupplierAsync(Guid supplierId);

        /// <summary>Si la empresa tiene alguna compra, anuladas incluidas.</summary>
        Task<bool> ExistsByCompanyAsync(Guid companyId);
    }
}
