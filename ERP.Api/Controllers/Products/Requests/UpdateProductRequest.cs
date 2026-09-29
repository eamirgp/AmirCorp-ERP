using ERP.Application.Features.Products.UpdateProduct;
using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    public sealed record UpdateProductRequest(
        string? Code,
        string? Name,
        UnitOfMeasure? UnitOfMeasure,
        IgvAffectation? IgvAffectation,
        decimal? SalePrice,
        uint? RowVersion
        )
    {
        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Code))
                errors.Add("El código es requerido.");

            if (!string.IsNullOrWhiteSpace(Code) && Code.Length > Product.CodeMaxLength)
                errors.Add($"El código no puede exceder los {Product.CodeMaxLength} caracteres.");

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add("El nombre es requerido.");

            if (!string.IsNullOrWhiteSpace(Name) && Name.Length > Product.NameMaxLength)
                errors.Add($"El nombre no puede exceder los {Product.NameMaxLength} caracteres.");

            if (UnitOfMeasure is null)
                errors.Add("La unidad de medida es requerida.");

            if (UnitOfMeasure is not null && !Enum.IsDefined(UnitOfMeasure.Value))
                errors.Add("La unidad de medida es inválida.");

            if (IgvAffectation is null)
                errors.Add("El tipo de afectación del IGV es requerido.");

            if (IgvAffectation is not null && !Enum.IsDefined(IgvAffectation.Value))
                errors.Add("El tipo de afectación del IGV es inválido.");

            if (SalePrice is null)
                errors.Add("Ingresa el precio de venta como un número, por ejemplo 12.90.");

            if (SalePrice is not null && SalePrice < 0)
                errors.Add("El precio de venta no puede ser negativo.");

            if (SalePrice is not null && SalePrice > Product.SalePriceMax)
                errors.Add("El precio de venta es demasiado grande. Revisa que esté bien escrito.");

            if (RowVersion is null)
                errors.Add("Falta la versión del producto. Vuelve a abrir el formulario.");

            return errors;
        }

        public UpdateProductDto ToDto(Guid id) =>
            new(id, Code!, Name!, UnitOfMeasure!.Value, IgvAffectation!.Value, SalePrice!.Value, RowVersion!.Value);
    }
}
