using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Products;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Domain.Purchases
{
    public sealed class PurchaseLine : BaseEntity
    {
        // Cantidades, unidades por caja y montos unitarios van en columnas numeric(18,6): 12 dígitos enteros y 6 decimales.
        public const decimal NumberMax = 999_999_999_999m;
        public const int Decimals = 6;
        // Tope del total de una línea: más es un error de tipeo, y así la suma de la compra cabe en su columna numeric(18,2).
        public const decimal LineTotalMax = 999_999_999_999.99m;

        public Guid PurchaseId { get; }
        public int LineNumber { get; }
        public Guid ProductId { get; }
        public string ProductCode { get; }
        public string ProductName { get; }
        /// <summary>
        /// Código del producto tal como viene en la factura del proveedor (null si la factura no trae código). Es una
        /// copia: si después se corrige el enlace en Productos, la compra sigue mostrando lo que decía su comprobante.
        /// </summary>
        public string? SupplierProductCode { get; }
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
            string? supplierProductCode,
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
            SupplierProductCode = supplierProductCode;
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
            string? supplierProductCode,
            InvoicePriceType invoicePriceType,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            decimal? conversionFactor
            )
        {
            ValidateProduct(productId);
            supplierProductCode = NormalizeSupplierProductCode(supplierProductCode);

            var amounts = Calculate(invoicePriceType, invoiceIgvAffectation, invoiceUnitOfMeasure, invoiceQuantity, invoiceAmount, conversionFactor);

            return new(
                Guid.CreateVersion7(),
                purchaseId,
                lineNumber,
                productId,
                productCode,
                productName,
                supplierProductCode,
                invoicePriceType,
                invoiceIgvAffectation,
                invoiceUnitOfMeasure.Code,
                invoiceQuantity,
                amounts.InvoiceUnitValue,
                amounts.InvoiceUnitPrice,
                amounts.ConversionFactor,
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
            decimal? requestedConversionFactor
            )
        {
            var (amounts, error) = Compute(invoicePriceType, invoiceIgvAffectation, invoiceUnitOfMeasure, invoiceQuantity, invoiceAmount, requestedConversionFactor);
            DomainException.ThrowIf(error);
            return amounts!;
        }

        /// <summary>
        /// Qué tiene de malo la línea para calcular sus montos, o null si está bien: lo mismo que revisa
        /// <see cref="Calculate"/>, sin lanzar el error. El registro y la vista previa la llaman antes, para mostrar el
        /// mensaje de cada línea.
        /// </summary>
        public static string? AmountsError(
            InvoicePriceType invoicePriceType,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            decimal? requestedConversionFactor
            ) =>
            Compute(invoicePriceType, invoiceIgvAffectation, invoiceUnitOfMeasure, invoiceQuantity, invoiceAmount, requestedConversionFactor).Error;

        /// <summary>
        /// El monto unitario tal como se escribió de la factura (lo que recibe <see cref="Calculate"/>): el valor sin IGV
        /// o el precio con IGV, según cómo vienen los montos de la factura.
        /// </summary>
        public static decimal InvoiceAmountFor(InvoicePriceType invoicePriceType, decimal invoiceUnitValue, decimal invoiceUnitPrice) =>
            invoicePriceType is InvoicePriceType.UnitPrice ? invoiceUnitPrice : invoiceUnitValue;

        /// <summary>Qué tiene de malo la afectación al IGV de la línea, o null si está bien (la misma regla del producto).</summary>
        public static string? InvoiceIgvAffectationError(IgvAffectation? invoiceIgvAffectation) =>
            Product.IgvAffectationError(invoiceIgvAffectation);

        /// <summary>Qué tiene de malo la cantidad de la factura, o null si está bien.</summary>
        public static string? InvoiceQuantityError(decimal? invoiceQuantity) =>
            invoiceQuantity switch
            {
                null => "La cantidad es requerida.",
                <= 0 => "La cantidad debe ser mayor a cero.",
                > NumberMax => "La cantidad es demasiado grande. Revisa que esté bien escrita.",
                { } value when HasTooManyDecimals(value) => $"La cantidad puede tener hasta {Decimals} decimales.",
                _ => null
            };

        /// <summary>Qué tiene de malo el monto unitario de la factura (valor o precio, según la compra), o null si está bien.</summary>
        public static string? InvoiceAmountError(decimal? invoiceAmount, InvoicePriceType invoicePriceType)
        {
            var label = $"El {invoicePriceType.Description.ToLowerInvariant()}";

            return invoiceAmount switch
            {
                null => $"{label} es requerido.",
                <= 0 => $"{label} debe ser mayor a cero.",
                > NumberMax => $"{label} es demasiado grande. Revisa que esté bien escrito.",
                { } value when HasTooManyDecimals(value) => $"{label} puede tener hasta {Decimals} decimales.",
                _ => null
            };
        }

        private static (PurchaseLineAmounts? Amounts, string? Error) Compute(
            InvoicePriceType invoicePriceType,
            IgvAffectation invoiceIgvAffectation,
            UnitOfMeasure invoiceUnitOfMeasure,
            decimal invoiceQuantity,
            decimal invoiceAmount,
            decimal? requestedConversionFactor
            )
        {
            if (!Enum.IsDefined(invoicePriceType))
                return (null, "El tipo de precio es inválido.");
            if (InvoiceIgvAffectationError(invoiceIgvAffectation) is { } igvError)
                return (null, igvError);
            if ((UnitOfMeasure.UsableError(invoiceUnitOfMeasure, invoiceUnitOfMeasure.Code)
                ?? InvoiceQuantityError(invoiceQuantity)
                ?? InvoiceAmountError(invoiceAmount, invoicePriceType)
                ?? ConversionFactorError(invoiceUnitOfMeasure, requestedConversionFactor)) is { } error)
                return (null, error);

            // Con una unidad de cantidad fija (Unidad 1, Docena 12) la pone el catálogo; con una variable (Caja), la factura.
            var conversionFactor = invoiceUnitOfMeasure.FixedConversionFactor ?? requestedConversionFactor!.Value;

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

            // Lo calculado también tiene que caber en sus columnas y tener sentido para el inventario.
            if (inventoryQuantity > NumberMax || HasTooManyDecimals(inventoryQuantity))
                return (null, "La cantidad en unidades no se puede registrar así. Revisa la cantidad y las unidades por caja.");
            if (total > LineTotalMax || invoiceUnitValue > NumberMax || invoiceUnitPrice > NumberMax)
                return (null, "El total de la línea es demasiado grande. Revisa la cantidad y el monto.");
            if (baseAmount == 0)
                return (null, "El subtotal de la línea sale 0.00. Revisa la cantidad y el monto.");

            var inventoryUnitCost = Math.Round(baseAmount / inventoryQuantity, Decimals, MidpointRounding.AwayFromZero);
            if (inventoryUnitCost == 0)
                return (null, "El costo de cada unidad sale 0. Revisa la cantidad, el monto y las unidades por caja.");
            // Muy pocas unidades para un monto grande (0.000001 unidades por caja): el costo no cabe en su columna.
            if (inventoryUnitCost > NumberMax)
                return (null, "El costo de cada unidad sale demasiado grande. Revisa la cantidad, el monto y las unidades por caja.");

            return (new PurchaseLineAmounts(invoiceUnitValue, invoiceUnitPrice, conversionFactor, inventoryQuantity, inventoryUnitCost, baseAmount, igvAmount, total), null);
        }

        /// <summary>
        /// Qué tiene de malo la cantidad de unidades por cada unidad de la factura, o null si está bien. Con una unidad
        /// de cantidad fija (Unidad 1, Docena 12) no hace falta indicarla: la pone el catálogo, y si se indica otra es
        /// un error. Con una variable (Caja), es obligatoria: solo la factura dice cuántas trae.
        /// La usa el dominio para rechazar la línea y el registro de la compra para avisar antes, junto con los demás errores.
        /// </summary>
        public static string? ConversionFactorError(UnitOfMeasure unitOfMeasure, decimal? conversionFactor)
        {
            var name = unitOfMeasure.Name.ToLowerInvariant();

            if (unitOfMeasure.FixedConversionFactor is { } fixedFactor)
                return conversionFactor is null || conversionFactor == fixedFactor
                    ? null
                    : $"Cada {name} trae {fixedFactor:0.######} unidades: no se puede indicar otra cantidad.";

            return conversionFactor switch
            {
                null => $"Indica cuántas unidades trae cada {name}.",
                <= 0 => $"Las unidades por {name} deben ser mayores a cero.",
                > NumberMax => $"Las unidades por {name} son demasiadas. Revisa que estén bien escritas.",
                { } value when HasTooManyDecimals(value) => $"Las unidades por {name} pueden tener hasta {Decimals} decimales.",
                _ => null
            };
        }

        // Más decimales de los que guarda la base: se perderían sin aviso al guardar.
        internal static bool HasTooManyDecimals(decimal value) =>
            Math.Round(value, Decimals) != value;

        private static void ValidateProduct(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new DomainException("El producto es requerido.");
        }

        /// <summary>Igual que el código guardado en el producto (mayúsculas, sin espacios de sobra); vacío es "sin código".</summary>
        private static string? NormalizeSupplierProductCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            DomainException.ThrowIf(ProductSupplierCode.CodeError(code));
            return ProductSupplierCode.NormalizeCode(code);
        }

    }
}
