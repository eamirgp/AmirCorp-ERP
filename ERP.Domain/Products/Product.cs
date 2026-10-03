using ERP.Domain.Catalogs;
using ERP.Domain.Common;
using ERP.Domain.Partners;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Domain.Products
{
    /// <summary>
    /// Producto del catálogo, compartido por todas las empresas. Las reglas se exponen como funciones <c>…Error</c>
    /// que devuelven el mensaje o null: el dominio lanza el error con ellas y la API, el importador de Excel y los
    /// casos de uso las llaman antes para avisar todos los errores juntos.
    /// </summary>
    public sealed class Product : AuditableEntity
    {
        // El código interno va en la factura electrónica, donde SUNAT acepta hasta 30 caracteres.
        public const int CodeMaxLength = 30;
        public const int NameMaxLength = 100;
        // La columna es numeric(18,6): 12 dígitos enteros como máximo.
        public const decimal SalePriceMax = 999_999_999_999m;
        // Precio en soles y céntimos (S/ 10.50): con más decimales se rechaza, no se redondea sin avisar.
        public const int SalePriceDecimals = 2;

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

        /// <param name="unitOfMeasure">La unidad del catálogo: debe estar activa.</param>
        public static Product Create(string code, string name, UnitOfMeasure unitOfMeasure, IgvAffectation igvAffectation, decimal salePrice)
        {
            Throw(CodeError(code));
            Throw(NameError(name));
            Throw(UnitOfMeasure.UsableError(unitOfMeasure, unitOfMeasure.Code));
            Throw(IgvAffectationError(igvAffectation));
            Throw(SalePriceError(salePrice));

            return new(Guid.CreateVersion7(), NormalizeCode(code), NormalizeName(name), unitOfMeasure.Code, igvAffectation, salePrice, isActive: true);
        }

        /// <summary>El código tal como se guarda y se compara: sin espacios alrededor y en mayúsculas (" abc" es "ABC").</summary>
        public static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();

        /// <summary>El nombre tal como se guarda: sin espacios al inicio ni al final, ni dobles en medio.</summary>
        public static string NormalizeName(string name) =>
            TextNormalizer.CollapseSpaces(name);

        public void UpdateCode(string code)
        {
            Throw(CodeError(code));
            Code = NormalizeCode(code);
        }

        public void UpdateName(string name)
        {
            Throw(NameError(name));
            Name = NormalizeName(name);
        }

        /// <summary>Cambia la unidad. Si es la misma, se acepta aunque ya no esté activa: el producto sigue editándose.</summary>
        public void UpdateUnitOfMeasure(UnitOfMeasure unitOfMeasure)
        {
            Throw(UnitOfMeasureChangeError(unitOfMeasure));
            UnitOfMeasureCode = unitOfMeasure.Code;
        }

        public void UpdateIgvAffectation(IgvAffectation igvAffectation)
        {
            Throw(IgvAffectationError(igvAffectation));
            IgvAffectation = igvAffectation;
        }

        public void UpdateSalePrice(decimal salePrice)
        {
            Throw(SalePriceError(salePrice));
            SalePrice = salePrice;
        }

        /// <summary>
        /// Reemplaza los códigos de proveedores por los indicados. Los que siguen conservan su registro, así el
        /// historial muestra solo lo que cambió.
        /// </summary>
        public void SetSupplierCodes(IReadOnlyCollection<(BusinessPartner Supplier, string Code)> codes)
        {
            if (SupplierCodesErrors(codes) is [var first, ..])
                throw new DomainException(first);

            var normalized = codes.Select(c => (SupplierId: c.Supplier.Id, Code: ProductSupplierCode.NormalizeCode(c.Code))).ToList();

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
            Throw(ProductSupplierCode.CodeError(code));
            Throw(SupplierCodeError(supplier, code));

            if (SupplierCodeOf(supplier.Id) is null)
                _supplierCodes.Add(ProductSupplierCode.Create(Id, supplier.Id, ProductSupplierCode.NormalizeCode(code)));
        }

        /// <summary>
        /// Qué impide enlazar ese código del proveedor al producto, o null si se puede. Un producto tiene un solo código
        /// por proveedor, y a un proveedor con compras bloqueadas no se le agregan códigos nuevos (conserva los que tenía).
        /// La usa <see cref="AddSupplierCode"/> y el registro de la compra para avisar antes, junto con los demás errores.
        /// </summary>
        public string? SupplierCodeError(BusinessPartner supplier, string code)
        {
            if (SupplierCodeOf(supplier.Id) is { } existing)
                return ConflictsWithLinkedCode(existing.Code, code)
                    ? $"El producto {Code} ya tiene el código {existing.Code} de {supplier.Name}. Si cambió, corrígelo desde Productos."
                    : null;

            return NewSupplierLinkError(supplier);
        }

        /// <summary>
        /// Si enlazar <paramref name="newCode"/> choca con el código que el producto ya tiene de ese proveedor (uno por
        /// proveedor). La usa <see cref="SupplierCodeError"/> y la búsqueda de una compra para no ofrecer ese producto.
        /// </summary>
        /// <param name="linkedCode">El código que ya tiene de ese proveedor, o null si no tiene.</param>
        public static bool ConflictsWithLinkedCode(string? linkedCode, string newCode) =>
            linkedCode is not null && linkedCode != ProductSupplierCode.NormalizeCode(newCode);

        /// <summary>
        /// Qué tiene de malo la lista completa de códigos del formulario, o una lista vacía si está bien: un código por
        /// proveedor, cada código bien escrito, y solo proveedores. Un proveedor con compras bloqueadas conserva su código
        /// (se puede corregir), pero no se le agrega uno nuevo. Que el código no lo use otro producto lo revisa quien
        /// puede buscar en la base.
        /// </summary>
        public IReadOnlyList<string> SupplierCodesErrors(IReadOnlyCollection<(BusinessPartner Supplier, string Code)> codes)
        {
            var errors = new List<string>();

            foreach (var group in codes.GroupBy(c => c.Supplier.Id))
            {
                var supplier = group.First().Supplier;

                if (group.Count() > 1)
                    errors.Add($"{supplier.Name} aparece más de una vez. Deja un solo código por proveedor.");

                foreach (var (_, code) in group)
                    if (ProductSupplierCode.CodeError(code) is { } codeError)
                        errors.Add($"{supplier.Name}: {codeError}");

                if (SupplierCodeOf(supplier.Id) is null && NewSupplierLinkError(supplier) is { } linkError)
                    errors.Add(linkError);
            }

            return errors;
        }

        public void Activate() =>
            IsActive = true;

        public void Deactivate() =>
            IsActive = false;

        /// <summary>Qué tiene de malo el código interno, o null si está bien.</summary>
        public static string? CodeError(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "El código interno es requerido.";

            if (NormalizeCode(code).Length > CodeMaxLength)
                return $"El código interno no puede exceder los {CodeMaxLength} caracteres.";

            return null;
        }

        /// <summary>Qué tiene de malo el nombre, o null si está bien.</summary>
        public static string? NameError(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "El nombre es requerido.";

            if (NormalizeName(name).Length > NameMaxLength)
                return $"El nombre no puede exceder los {NameMaxLength} caracteres.";

            return null;
        }

        /// <summary>Qué tiene de malo la afectación al IGV, o null si está bien.</summary>
        public static string? IgvAffectationError(IgvAffectation? igvAffectation) =>
            igvAffectation switch
            {
                null => "El tipo de afectación del IGV es requerido.",
                { } value when !Enum.IsDefined(value) => "El tipo de afectación del IGV es inválido.",
                _ => null
            };

        /// <summary>Qué tiene de malo el precio de venta, o null si está bien. Puede ser 0 (producto todavía sin precio).</summary>
        public static string? SalePriceError(decimal? salePrice) =>
            salePrice switch
            {
                null => "Ingresa el precio de venta como un número, por ejemplo 12.90.",
                < 0 => "El precio de venta no puede ser negativo.",
                > SalePriceMax => "El precio de venta es demasiado grande. Revisa que esté bien escrito.",
                { } value when Math.Round(value, SalePriceDecimals) != value =>
                    $"El precio de venta puede tener hasta {SalePriceDecimals} decimales (céntimos), por ejemplo 12.90.",
                _ => null
            };

        /// <summary>Qué impide cambiar la unidad por esa, o null si se puede (la misma unidad se acepta aunque esté desactivada).</summary>
        public string? UnitOfMeasureChangeError(UnitOfMeasure unitOfMeasure) =>
            unitOfMeasure.Code == UnitOfMeasureCode ? null : UnitOfMeasure.UsableError(unitOfMeasure, unitOfMeasure.Code);

        // A un proveedor solo se le enlaza un código nuevo si es proveedor y no tiene las compras bloqueadas.
        private static string? NewSupplierLinkError(BusinessPartner supplier) =>
            !supplier.IsSupplier ? $"{supplier.Name} no está registrado como proveedor." : supplier.PurchasingBlockedError();

        private static void Throw(string? error)
        {
            if (error is not null)
                throw new DomainException(error);
        }
    }
}
