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
        /// <param name="product">El producto que se crea o se edita (el nuevo aún sin códigos).</param>
        /// <returns>Cada código con su proveedor, listos para <see cref="Product.SetSupplierCodes"/>.</returns>
        public static async Task<Result<IReadOnlyCollection<(BusinessPartner Supplier, string Code)>>> CheckAsync(
            IReadOnlyCollection<ProductSupplierCodeDto> codes,
            Product product,
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

            if (product.SupplierCodesErrors(resolved) is { Count: > 0 } errors)
                return Failure(errors, ErrorType.BadRequest);

            var inUse = await productRepository.SupplierCodesInUseAsync(codes.Select(c => (c.SupplierId, c.Code)).ToArray(), product.Id);
            if (inUse.Count > 0)
                return Failure(
                    inUse.Select(c => $"El código {c.Code} de {suppliers[c.SupplierId].Name} ya está en el producto {c.ProductCode} · {c.ProductName}.").ToArray(),
                    ErrorType.Conflict
                    );

            return Result<IReadOnlyCollection<(BusinessPartner, string)>>.Success(resolved);
        }

        private static Result<IReadOnlyCollection<(BusinessPartner, string)>> Failure(IReadOnlyCollection<string> errors, ErrorType type) =>
            Result<IReadOnlyCollection<(BusinessPartner, string)>>.Failure(errors, type);
    }
}
