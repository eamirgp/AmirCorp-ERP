using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.ListProducts
{
    public sealed record ListProductsResponseDto(
        Guid Id,
        string Code,
        string Name,
        string UnitOfMeasureCode,
        string UnitOfMeasureName,
        IgvAffectation IgvAffectation,
        decimal SalePrice,
        bool IsActive,
        // Ordenados por nombre del proveedor. Los usa el formulario; la lista no los muestra.
        IReadOnlyCollection<ProductSupplierCodeResponseDto> SupplierCodes,
        // Código del proveedor pedido en SupplierId (el de la compra), o null.
        string? SupplierCode,
        // Proveedor cuyo código coincidió con lo buscado, solo si no coincidió el código interno ni el nombre.
        Guid? MatchedSupplierId,
        // Versión del producto: el formulario la devuelve al editar para no pisar cambios de otra persona.
        uint RowVersion,
        // El código de la factura que se quiere enlazar (LinkCode de la búsqueda), o null.
        string? LinkCode = null
        )
    {
        public string IgvAffectationDescription => IgvAffectation.Description;
        public string IgvAffectationShortDescription => IgvAffectation.ShortDescription;

        /// <summary>"Activo" o "Inactivo", para la columna Estado.</summary>
        public string StatusDescription => IsActive ? "Activo" : "Inactivo";

        /// <summary>
        /// En una compra, al enlazar el código <see cref="LinkCode"/> de la factura: por qué este producto no se puede
        /// elegir (ya tiene otro código de ese proveedor; la regla es <see cref="Product.ConflictsWithLinkedCode"/>), o null.
        /// </summary>
        public string? LinkError =>
            LinkCode is { } code && Product.ConflictsWithLinkedCode(SupplierCode, code)
                ? $"Ya tiene el código {SupplierCode} de este proveedor. Si cambió, corrígelo en Productos."
                : null;

        /// <summary>
        /// Por qué apareció en la búsqueda, solo cuando fue por un código de proveedor:
        /// "Encontrado por el código YH-2045-BK de Proveedor X". Si coincide el código interno o el nombre, null.
        /// </summary>
        public string? SearchMatch =>
            SupplierCodes.FirstOrDefault(c => c.SupplierId == MatchedSupplierId) is { } match
                ? $"Encontrado por el código {match.Code} de {match.SupplierName}"
                : null;
    }
}
