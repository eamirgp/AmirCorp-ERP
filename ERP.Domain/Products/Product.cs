using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Partners;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Domain.Products
{
    public sealed class Product : AuditableEntity
    {
        // El código interno va en la factura electrónica, donde SUNAT acepta hasta 30 caracteres.
        public const int CodeMaxLength = 30;
        public const int NameMaxLength = 100;
        // La columna es numeric(18,6): 12 dígitos enteros como máximo.
        public const decimal SalePriceMax = 999_999_999_999m;

        /// <summary>Código interno: lo define la empresa y es único.</summary>
        public string Code { get; private set; }
        public string Name { get; private set; }
        /// <summary>Código SUNAT de la unidad de medida (NIU, DZN…). Debe ser una unidad activa del catálogo.</summary>
        public string UnitOfMeasureCode { get; private set; }
        public IgvAffectation IgvAffectation { get; private set; }
        public decimal SalePrice { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<ProductSupplierCode> _supplierCodes = [];
        /// <summary>Códigos con los que cada proveedor identifica al producto: uno por proveedor.</summary>
        public IReadOnlyCollection<ProductSupplierCode> SupplierCodes => _supplierCodes.AsReadOnly();

        private Product(Guid id, string code, string name, string unitOfMeasureCode, IgvAffectation igvAffectation, decimal salePrice, bool isActive) : base(id)
        {
            Code = code;
            Name = name;
            UnitOfMeasureCode = unitOfMeasureCode;
            IgvAffectation = igvAffectation;
            SalePrice = salePrice;
            IsActive = isActive;
        }

        public static Product Create(string code, string name, string unitOfMeasureCode, IgvAffectation igvAffectation, decimal salePrice) =>
            new(Guid.CreateVersion7(), ValidateCode(code), ValidateName(name), ValidateUnitOfMeasureCode(unitOfMeasureCode), ValidateIgvAffectation(igvAffectation), ValidateSalePrice(salePrice), isActive: true);

        /// <summary>El código tal como se guarda y se compara: sin espacios alrededor y en mayúsculas (" abc" es "ABC").</summary>
        public static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();

        /// <summary>El nombre tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        public void UpdateCode(string code) =>
            Code = ValidateCode(code);

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void UpdateUnitOfMeasure(string unitOfMeasureCode) =>
            UnitOfMeasureCode = ValidateUnitOfMeasureCode(unitOfMeasureCode);

        public void UpdateIgvAffectation(IgvAffectation igvAffectation) =>
            IgvAffectation = ValidateIgvAffectation(igvAffectation);

        public void UpdateSalePrice(decimal salePrice) =>
            SalePrice = ValidateSalePrice(salePrice);

        /// <summary>
        /// Reemplaza los códigos de proveedores por los indicados. Los que siguen conservan su registro, así el
        /// historial muestra solo lo que cambió.
        /// </summary>
        public void SetSupplierCodes(IReadOnlyCollection<(Guid SupplierId, string Code)> codes)
        {
            var normalized = codes.Select(c => (c.SupplierId, Code: ValidateSupplierCode(c.SupplierId, c.Code))).ToList();

            if (normalized.GroupBy(c => c.SupplierId).Any(g => g.Count() > 1))
                throw new DomainException("Un proveedor aparece más de una vez en los códigos de proveedores. Deja un solo código por proveedor.");

            _supplierCodes.RemoveAll(existing => normalized.All(c => c.SupplierId != existing.SupplierId));

            foreach (var (supplierId, code) in normalized)
            {
                var existing = SupplierCodeOf(supplierId);
                if (existing is null)
                    _supplierCodes.Add(ProductSupplierCode.Create(Id, supplierId, code));
                else if (existing.Code != code)
                    existing.UpdateCode(code);
            }
        }

        /// <summary>El código con que ese proveedor identifica al producto, o null si no está enlazado.</summary>
        public ProductSupplierCode? SupplierCodeOf(Guid supplierId) =>
            _supplierCodes.FirstOrDefault(c => c.SupplierId == supplierId);

        /// <summary>
        /// Enlaza el código con que un proveedor vende este producto (por ejemplo, desde una compra).
        /// Si ya tiene ese mismo código no cambia nada; si tiene otro de ese proveedor, se corrige desde Productos.
        /// </summary>
        public void AddSupplierCode(BusinessPartner supplier, string code)
        {
            var normalized = ValidateSupplierCode(supplier.Id, code);
            if (SupplierCodeError(supplier, normalized) is { } error)
                throw new DomainException(error);

            if (SupplierCodeOf(supplier.Id) is null)
                _supplierCodes.Add(ProductSupplierCode.Create(Id, supplier.Id, normalized));
        }

        /// <summary>
        /// Qué impide enlazar ese código del proveedor al producto, o null si se puede. Un producto tiene un solo código
        /// por proveedor, y a un proveedor con compras bloqueadas no se le agregan códigos nuevos (conserva los que tenía).
        /// La usa <see cref="AddSupplierCode"/> y el registro de la compra para avisar antes, junto con los demás errores.
        /// </summary>
        public string? SupplierCodeError(BusinessPartner supplier, string code)
        {
            if (SupplierCodeOf(supplier.Id) is { } existing)
                return existing.Code == ProductSupplierCode.NormalizeCode(code)
                    ? null
                    : $"El producto {Code} ya tiene el código {existing.Code} de {supplier.Name}. Si cambió, corrígelo desde Productos.";

            if (!supplier.IsSupplier)
                return $"{supplier.Name} no está registrado como proveedor.";

            return supplier.PurchasingBlockedError();
        }

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static string ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("El código interno es requerido.");

            var normalized = NormalizeCode(code);
            if (normalized.Length > CodeMaxLength)
                throw new DomainException($"El código interno no puede exceder los {CodeMaxLength} caracteres.");

            return normalized;
        }

        private static string ValidateSupplierCode(Guid supplierId, string code)
        {
            if (supplierId == Guid.Empty)
                throw new DomainException("Cada código de proveedor necesita su proveedor.");

            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("Falta el código en uno de los proveedores.");

            var normalized = ProductSupplierCode.NormalizeCode(code);
            if (normalized.Length > ProductSupplierCode.CodeMaxLength)
                throw new DomainException($"El código de proveedor no puede exceder los {ProductSupplierCode.CodeMaxLength} caracteres.");

            return normalized;
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre es requerido.");

            var normalized = NormalizeName(name);
            if (normalized.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return normalized;
        }

        // Que la unidad exista y esté activa lo revisa el caso de uso, que conoce el catálogo.
        private static string ValidateUnitOfMeasureCode(string unitOfMeasureCode)
        {
            if (string.IsNullOrWhiteSpace(unitOfMeasureCode))
                throw new DomainException("La unidad de medida es requerida.");

            var code = UnitOfMeasure.NormalizeCode(unitOfMeasureCode);
            if (code.Length > UnitOfMeasure.CodeMaxLength)
                throw new DomainException("La unidad de medida es inválida.");

            return code;
        }

        private static IgvAffectation ValidateIgvAffectation(IgvAffectation igvAffectation)
        {
            if (!Enum.IsDefined(igvAffectation))
                throw new DomainException("El tipo de afectación del IGV es inválido.");

            return igvAffectation;
        }

        private static decimal ValidateSalePrice(decimal salePrice)
        {
            if (salePrice < 0)
                throw new DomainException("El precio de venta no puede ser negativo.");

            if (salePrice > SalePriceMax)
                throw new DomainException("El precio de venta es demasiado grande. Revisa que esté bien escrito.");

            return salePrice;
        }
    }
}
