using ERP.Application.Features.Products.UpdateProduct;
using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    public sealed record UpdateProductRequest(
        string? Code,
        string? Name,
        string? UnitOfMeasureCode,
        IgvAffectation? IgvAffectation,
        decimal? SalePrice,
        // Lista completa de códigos de proveedores: los que no se envían se quitan del producto.
        IReadOnlyCollection<ProductSupplierCodeRequest?>? SupplierCodes,
        uint? RowVersion
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = ProductRequestRules.Validate(Code, Name, UnitOfMeasureCode, IgvAffectation, SalePrice, SupplierCodes);

            if (RowVersion is null)
                errors.Add("Falta la versión del producto. Vuelve a abrir el formulario.");

            return errors;
        }

        public UpdateProductDto ToDto(Guid id) =>
            new(id, Code!, Name!, UnitOfMeasureCode!, IgvAffectation!.Value, SalePrice!.Value, ProductSupplierCodeRequest.ToDtos(SupplierCodes), RowVersion!.Value);
    }
}
