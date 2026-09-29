using ERP.Domain.Catalogs;
using ERP.Domain.Common;

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
        public UnitOfMeasure UnitOfMeasure { get; private set; }
        public IgvAffectation IgvAffectation { get; private set; }
        public decimal SalePrice { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<ProductSupplierCode> _supplierCodes = [];
        /// <summary>Códigos con los que cada proveedor identifica al producto: uno por proveedor.</summary>
        public IReadOnlyCollection<ProductSupplierCode> SupplierCodes => _supplierCodes.AsReadOnly();

        private Product(Guid id, string code, string name, UnitOfMeasure unitOfMeasure, IgvAffectation igvAffectation, decimal salePrice, bool isActive) : base(id)
        {
            Code = code;
            Name = name;
            UnitOfMeasure = unitOfMeasure;
            IgvAffectation = igvAffectation;
            SalePrice = salePrice;
            IsActive = isActive;
        }

        public static Product Create(string code, string name, UnitOfMeasure unitOfMeasure, IgvAffectation igvAffectation, decimal salePrice) =>
            new(Guid.CreateVersion7(), ValidateCode(code), ValidateName(name), ValidateUnitOfMeasure(unitOfMeasure), ValidateIgvAffectation(igvAffectation), ValidateSalePrice(salePrice), isActive: true);

        public static string NormalizeCode(string code) =>
            code.ToUpperInvariant();

        public void UpdateCode(string code) =>
            Code = ValidateCode(code);

        public void UpdateName(string name) =>
            Name = ValidateName(name);

        public void UpdateUnitOfMeasure(UnitOfMeasure unitOfMeasure) =>
            UnitOfMeasure = ValidateUnitOfMeasure(unitOfMeasure);

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
                var existing = _supplierCodes.FirstOrDefault(c => c.SupplierId == supplierId);
                if (existing is null)
                    _supplierCodes.Add(ProductSupplierCode.Create(Id, supplierId, code));
                else if (existing.Code != code)
                    existing.UpdateCode(code);
            }
        }

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        private static string ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("El código interno es requerido.");

            if (code.Length > CodeMaxLength)
                throw new DomainException($"El código interno no puede exceder los {CodeMaxLength} caracteres.");

            return NormalizeCode(code);
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

            if (name.Length > NameMaxLength)
                throw new DomainException($"El nombre no puede exceder los {NameMaxLength} caracteres.");

            return name;
        }

        private static UnitOfMeasure ValidateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
        {
            if (!Enum.IsDefined(unitOfMeasure))
                throw new DomainException("La unidad de medida es inválida.");

            return unitOfMeasure;
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
