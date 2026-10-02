using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Domain.Purchases
{
    public sealed class PurchaseLine : BaseEntity
    {
        public Guid PurchaseId { get; }
        public int LineNumber { get; }
        public Guid ProductId { get; }
        public string ProductCode { get; }
        public string ProductName { get; }
        public InvoicePriceType InvoicePriceType { get; }
        public IgvAffectation InvoiceIgvAffectation { get; }
        /// <summary>Código SUNAT de la unidad en que viene la factura (NIU, DZN, BX…).</summary>
        public string InvoiceUnitOfMeasureCode { get; }
        public decimal InvoiceQuantity { get; }
        public decimal InvoiceUnitValue { get; }
        public decimal InvoiceUnitPrice { get; }
        public decimal ConversionFactor { get; }
        public decimal InventoryQuantity { get; }
        public decimal InventoryUnitCost { get; }
        public decimal BaseAmount { get; }
        public decimal IgvAmount { get; }
        public decimal Total { get; }

        private PurchaseLine(
            Guid id,
            Guid purchaseId,
            int lineNumber,
            Guid productId,
            string productCode,
            string productName,
            InvoicePriceType invoicePriceType,
            IgvAffectation invoiceIgvAffectation,
            string invoiceUnitOfMeasureCode,
            decimal invoiceQuantity,
            decimal invoiceUnitValue,
            decimal invoiceUnitPrice,
            decimal conversionFactor,
            decimal inventoryQuantity,
            decimal inventoryUnitCost,
            decimal baseAmount,
            decimal igvAmount,
            decimal total
            ) : base(id)
        {
            PurchaseId = purchaseId;
            LineNumber = lineNumber;
            ProductId = productId;
            ProductCode = productCode;
            ProductName = productName;
            InvoicePriceType = invoicePriceType;
            InvoiceIgvAffectation = invoiceIgvAffectation;
            InvoiceUnitOfMeasureCode = invoiceUnitOfMeasureCode;
            InvoiceQuantity = invoiceQuantity;
            InvoiceUnitValue = invoiceUnitValue;
            InvoiceUnitPrice = invoiceUnitPrice;
            ConversionFactor = conversionFactor;
            InventoryQuantity = inventoryQuantity;
            InventoryUnitCost = inventoryUnitCost;
            BaseAmount = baseAmount;
            IgvAmount = igvAmount;
            Total = total;
        }

        internal static PurchaseLine Create(
            Guid purchaseId,
            int lineNumber,
            Guid productId,
            string productCode,
            string productName,
            InvoicePriceType invoicePriceType,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            decimal conversionFactor
            )
        {
            ValidateProduct(productId);

            var amounts = Calculate(invoicePriceType, invoiceIgvAffectation, invoiceUnitOfMeasure, invoiceQuantity, invoiceAmount, conversionFactor);

            if (!invoiceUnitOfMeasure.IsActive)
                throw new DomainException($"La unidad de medida '{invoiceUnitOfMeasure.Name}' está desactivada.");

            return new(
                Guid.CreateVersion7(),
                purchaseId,
                lineNumber,
                productId,
                productCode,
                productName,
                invoicePriceType,
                invoiceIgvAffectation,
                invoiceUnitOfMeasure.Code,
                invoiceQuantity,
                amounts.InvoiceUnitValue,
                amounts.InvoiceUnitPrice,
                conversionFactor,
                amounts.InventoryQuantity,
                amounts.InventoryUnitCost,
                amounts.BaseAmount,
                amounts.IgvAmount,
                amounts.Total
                );
        }

        /// <summary>
        /// Valida y calcula los montos de una línea sin crearla. Es la única fórmula de cálculo:
        /// la usan tanto el registro de la compra como su vista previa.
        /// </summary>
        public static PurchaseLineAmounts Calculate(
            InvoicePriceType invoicePriceType,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            decimal conversionFactor
            )
        {
            ValidateInvoicePriceType(invoicePriceType);
            ValidateInvoiceIgvAffectation(invoiceIgvAffectation);
            ValidateInvoiceQuantity(invoiceQuantity);
            ValidateInvoiceAmount(invoiceAmount, invoicePriceType);
            ValidateConversionFactor(conversionFactor, invoiceUnitOfMeasure);

            var rate = invoiceIgvAffectation.Rate;

            decimal invoiceUnitValue, invoiceUnitPrice, baseAmount, igvAmount, total;

            if (invoicePriceType is InvoicePriceType.UnitPrice)
            {
                invoiceUnitPrice = invoiceAmount;
                invoiceUnitValue = Math.Round(invoiceUnitPrice / (1 + rate), 6, MidpointRounding.AwayFromZero);
                total = Math.Round(invoiceUnitPrice * invoiceQuantity, 2, MidpointRounding.AwayFromZero);
                baseAmount = Math.Round(total / (1 + rate), 2, MidpointRounding.AwayFromZero);
                igvAmount = total - baseAmount;
            }
            else
            {
                invoiceUnitValue = invoiceAmount;
                invoiceUnitPrice = Math.Round(invoiceUnitValue * (1 + rate), 6, MidpointRounding.AwayFromZero);
                baseAmount = Math.Round(invoiceUnitValue * invoiceQuantity, 2, MidpointRounding.AwayFromZero);
                igvAmount = Math.Round(baseAmount * rate, 2, MidpointRounding.AwayFromZero);
                total = baseAmount + igvAmount;
            }

            var inventoryQuantity = invoiceQuantity * conversionFactor;
            var inventoryUnitCost = Math.Round(baseAmount / inventoryQuantity, 6, MidpointRounding.AwayFromZero);

            return new PurchaseLineAmounts(invoiceUnitValue, invoiceUnitPrice, inventoryQuantity, inventoryUnitCost, baseAmount, igvAmount, total);
        }

        private static void ValidateProduct(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new DomainException("El producto es requerido.");
        }

        private static void ValidateInvoicePriceType(InvoicePriceType invoicePriceType)
        {
            if (!Enum.IsDefined(invoicePriceType))
                throw new DomainException("El tipo de precio es inválido.");
        }

        private static void ValidateInvoiceIgvAffectation(IgvAffectation invoiceIgvAffectation)
        {
            if (!Enum.IsDefined(invoiceIgvAffectation))
                throw new DomainException("El tipo de afectación del IGV es inválido.");
        }

        private static void ValidateInvoiceQuantity(decimal invoiceQuantity)
        {
            if (invoiceQuantity <= 0)
                throw new DomainException("La cantidad debe ser mayor a cero.");
        }

        private static void ValidateInvoiceAmount(decimal invoiceAmount, InvoicePriceType invoicePriceType)
        {
            if (invoiceAmount <= 0)
                throw new DomainException($"El {invoicePriceType.Description.ToLowerInvariant()} debe ser mayor a cero.");
        }

        private static void ValidateConversionFactor(decimal conversionFactor, UnitOfMeasure unitOfMeasure)
        {
            if (conversionFactor <= 0)
                throw new DomainException($"Las unidades por {unitOfMeasure.Name.ToLowerInvariant()} deben ser mayores a cero.");

            var fixedFactor = unitOfMeasure.FixedConversionFactor;
            if (fixedFactor is not null && conversionFactor != fixedFactor)
                throw new DomainException($"Cada {unitOfMeasure.Name.ToLowerInvariant()} trae {fixedFactor.Value:0.######} unidades: no se puede indicar otra cantidad.");
        }
    }
}
