using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    /// <summary>
    /// Revisión previa de un producto, igual al crear y al editar: las mismas reglas del dominio
    /// (<see cref="Product.CodeError"/>, <see cref="Product.NameError"/>…), juntas para avisar todos los errores de una vez.
    /// </summary>
    internal static class ProductRequestRules
    {
        public static List<string> Validate(
            string? code,
            string? name,
            string? unitOfMeasureCode,
            IgvAffectation? igvAffectation,
            decimal? salePrice,
            IReadOnlyCollection<ProductSupplierCodeRequest?>? supplierCodes)
        {
            var errors = new List<string>();

            if (Product.CodeError(code) is { } codeError)
                errors.Add(codeError);

            if (Product.NameError(name) is { } nameError)
                errors.Add(nameError);

            // Que la unidad exista y esté activa lo revisa el registro, que conoce el catálogo.
            if (string.IsNullOrWhiteSpace(unitOfMeasureCode))
                errors.Add("La unidad de medida es requerida.");

            if (Product.IgvAffectationError(igvAffectation) is { } igvError)
                errors.Add(igvError);

            if (Product.SalePriceError(salePrice) is { } priceError)
                errors.Add(priceError);

            errors.AddRange(ProductSupplierCodeRequest.Validate(supplierCodes));

            return errors;
        }
    }
}
