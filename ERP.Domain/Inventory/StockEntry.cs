using ERP.Domain.Common;

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
            if (originalQuantity <= 0)
                throw new DomainException("La cantidad que ingresa al stock debe ser mayor a cero.");
            if (unitCost < 0)
                throw new DomainException("El costo de cada unidad no puede ser negativo.");
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
            if (quantity <= 0)
                throw new DomainException("La cantidad a consumir debe ser mayor a 0.");

            if (quantity > RemainingQuantity)
                throw new DomainException("La cantidad supera el stock disponible en ese lote.");

            RemainingQuantity -= quantity;
        }
    }
}
