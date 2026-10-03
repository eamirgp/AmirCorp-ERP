using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Catalogs;

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
        uint RowVersion
        )
    {
        public string IgvAffectationDescription => IgvAffectation.Description;
        public string IgvAffectationShortDescription => IgvAffectation.ShortDescription;

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
