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
            ) =>
            new(
                Guid.CreateVersion7(),
                companyId, productId,
                purchaseLineId,
                originalQuantity,
                unitCost,
                entryDate
                );

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
