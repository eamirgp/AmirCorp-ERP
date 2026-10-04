using ERP.Application.Common.Results;
using ERP.Application.Features.Products.CreateProduct;
using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    public sealed record CreateProductRequest(
        string? Code,
        string? Name,
        string? UnitOfMeasureCode,
        IgvAffectation? IgvAffectation,
        decimal? SalePrice,
        IReadOnlyCollection<ProductSupplierCodeRequest?>? SupplierCodes
        )
    {
        public IReadOnlyCollection<ErrorDetail> Validate() =>
            ProductRequestRules.Validate(Code, Name, UnitOfMeasureCode, IgvAffectation, SalePrice, SupplierCodes);

        public CreateProductDto ToDto() =>
            new(Code!, Name!, UnitOfMeasureCode!, IgvAffectation!.Value, SalePrice!.Value, ProductSupplierCodeRequest.ToDtos(SupplierCodes));
    }
}
