using ERP.Application.Common.Results;
using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    /// <summary>
    /// Revisión previa de un producto, igual al crear y al editar: las mismas reglas del dominio
    /// (<see cref="Product.CodeError"/>, <see cref="Product.NameError"/>…), juntas para avisar todos los errores de una vez,
    /// cada uno con su campo (decisión 37).
    /// </summary>
    internal static class ProductRequestRules
    {
        public static List<ErrorDetail> Validate(
            string? code,
            string? name,
            string? unitOfMeasureCode,
            IgvAffectation? igvAffectation,
            decimal? salePrice,
            IReadOnlyCollection<ProductSupplierCodeRequest?>? supplierCodes)
        {
            var errors = new List<ErrorDetail>();

            if (Product.CodeError(code) is { } codeError)
                errors.Add(new(codeError, FieldName.Of(nameof(CreateProductRequest.Code))));

            if (Product.NameError(name) is { } nameError)
                errors.Add(new(nameError, FieldName.Of(nameof(CreateProductRequest.Name))));

            // Que la unidad exista y esté activa lo revisa el registro, que conoce el catálogo.
            if (string.IsNullOrWhiteSpace(unitOfMeasureCode))
                errors.Add(new("Elige la unidad de medida.", FieldName.Of(nameof(CreateProductRequest.UnitOfMeasureCode))));

            if (Product.IgvAffectationError(igvAffectation) is { } igvError)
                errors.Add(new(igvError, FieldName.Of(nameof(CreateProductRequest.IgvAffectation))));

            if (Product.SalePriceError(salePrice) is { } priceError)
                errors.Add(new(priceError, FieldName.Of(nameof(CreateProductRequest.SalePrice))));

            errors.AddRange(ProductSupplierCodeRequest.Validate(supplierCodes));

            return errors;
        }
    }
}
