using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.SupplierCodes
{
    /// <summary>
    /// Prepara los códigos de proveedores del formulario: busca a cada proveedor, revisa las reglas del producto
    /// (<see cref="Product.SupplierCodesErrors"/>) y, como necesita la base, que el código no lo use otro producto.
    /// </summary>
    internal static class ProductSupplierCodeRules
    {
        /// <param name="product">El producto que se edita, o null si se está creando (aún no tiene códigos).</param>
        /// <returns>Cada código con su proveedor, listos para <see cref="Product.SetSupplierCodes"/>.</returns>
        public static async Task<Result<IReadOnlyCollection<(BusinessPartner Supplier, string Code)>>> CheckAsync(
            IReadOnlyCollection<ProductSupplierCodeDto> codes,
            Product? product,
            IBusinessPartnerRepository businessPartnerRepository,
            IProductRepository productRepository
            )
        {
            if (codes.Count == 0)
                return Result<IReadOnlyCollection<(BusinessPartner, string)>>.Success([]);

            var suppliers = (await businessPartnerRepository.GetByIdsAsync(codes.Select(c => c.SupplierId).Distinct().ToArray()))
                .ToDictionary(s => s.Id);

            if (codes.Any(c => !suppliers.ContainsKey(c.SupplierId)))
                return Failure(["Uno de los proveedores elegidos ya no existe. Quítalo y elige otro."], ErrorType.BadRequest);

            var resolved = codes.Select(c => (suppliers[c.SupplierId], c.Code)).ToArray();

            var errors = product is null ? Product.NewProductSupplierCodesErrors(resolved) : product.SupplierCodesErrors(resolved);
            if (errors.Count > 0)
                return Failure(errors, ErrorType.BadRequest);

            var inUse = await productRepository.SupplierCodesInUseAsync(codes.Select(c => (c.SupplierId, c.Code)).ToArray(), product?.Id);
            if (inUse.Count > 0)
                return Failure(
                    inUse.Select(c => ProductSupplierCode.CodeTakenError(c.Code, suppliers[c.SupplierId].Name, c.ProductCode, c.ProductName, c.ProductIsActive)).ToArray(),
                    ErrorType.Conflict
                    );

            return Result<IReadOnlyCollection<(BusinessPartner, string)>>.Success(resolved);
        }

        private static Result<IReadOnlyCollection<(BusinessPartner, string)>> Failure(IReadOnlyCollection<string> errors, ErrorType type) =>
            Result<IReadOnlyCollection<(BusinessPartner, string)>>.Failure(errors, type);
    }
}
