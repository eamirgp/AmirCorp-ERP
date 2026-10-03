using ERP.Domain.Common;
using ERP.Domain.Purchases;

namespace ERP.Domain.Inventory
{
    public sealed class StockEntry : BaseEntity
    {
        public Guid CompanyId { get; }
        public Guid ProductId { get; }
        public Guid PurchaseLineId { get; }
        public decimal OriginalQuantity { get; }
        public decimal RemainingQuantity { get; private set; }
        public decimal UnitCost { get; }
        public DateOnly EntryDate { get; }
        public bool IsIntact => RemainingQuantity == OriginalQuantity;

        private StockEntry(
            Guid id,
            Guid companyId,
            Guid productId,
            Guid purchaseLineId,
            decimal originalQuantity,
            decimal unitCost,
            DateOnly entryDate
            ) : base(id)
        {
            CompanyId = companyId;
            ProductId = productId;
            PurchaseLineId = purchaseLineId;
            OriginalQuantity = originalQuantity;
            RemainingQuantity = originalQuantity;
            UnitCost = unitCost;
            EntryDate = entryDate;
        }

        /// <summary>
        /// Lo que entra al stock por una línea de compra: sus unidades y su costo por unidad (sin IGV), en la empresa que
        /// compra y con la fecha de emisión del comprobante.
        /// </summary>
        public static StockEntry FromPurchaseLine(Purchase purchase, PurchaseLine line)
        {
            if (purchase.IsCancelled)
                throw new DomainException("Una compra anulada no ingresa mercadería al stock.");

            if (line.PurchaseId != purchase.Id)
                throw new DomainException("La línea no es de esta compra.");

            return Create(purchase.CompanyId, line.ProductId, line.Id, line.InventoryQuantity, line.InventoryUnitCost, purchase.IssueDate);
        }

        public static StockEntry Create(
            Guid companyId,
            Guid productId,
            Guid purchaseLineId,
            decimal originalQuantity,
            decimal unitCost,
            DateOnly entryDate
            )
        {
            if (companyId == Guid.Empty)
                throw new DomainException("La empresa del ingreso de stock es requerida.");
            if (productId == Guid.Empty)
                throw new DomainException("El producto del ingreso de stock es requerido.");
            if (purchaseLineId == Guid.Empty)
                throw new DomainException("La línea de compra del ingreso de stock es requerida.");
            DomainException.ThrowIf(QuantityError(originalQuantity, "La cantidad que ingresa al stock"));
            // El costo sale de la línea de compra, que ya lo revisó: aquí se cuida que nunca quede uno imposible.
            if (unitCost <= 0)
                throw new DomainException("El costo de cada unidad debe ser mayor a cero.");
            if (unitCost > PurchaseLine.NumberMax || PurchaseLine.HasTooManyDecimals(unitCost))
                throw new DomainException($"El costo de cada unidad debe tener hasta {PurchaseLine.Decimals} decimales y no ser demasiado grande.");
            if (entryDate == default)
                throw new DomainException("La fecha del ingreso de stock es requerida.");

            return new(
                Guid.CreateVersion7(),
                companyId, productId,
                purchaseLineId,
                originalQuantity,
                unitCost,
                entryDate
                );
        }

        public void Consume(decimal quantity)
        {
            DomainException.ThrowIf(QuantityError(quantity, "La cantidad a consumir"));

            if (quantity > RemainingQuantity)
                throw new DomainException("La cantidad supera el stock disponible en ese lote.");

            RemainingQuantity -= quantity;
        }

        // Las cantidades van en columnas numeric(18,6), como las de la línea de compra.
        private static string? QuantityError(decimal quantity, string label) =>
            quantity switch
            {
                <= 0 => $"{label} debe ser mayor a cero.",
                > PurchaseLine.NumberMax => $"{label} es demasiado grande.",
                _ when PurchaseLine.HasTooManyDecimals(quantity) => $"{label} puede tener hasta {PurchaseLine.Decimals} decimales.",
                _ => null
            };
    }
}
