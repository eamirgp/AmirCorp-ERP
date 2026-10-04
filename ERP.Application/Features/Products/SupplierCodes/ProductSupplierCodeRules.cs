using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.CreateProduct;
using ERP.Domain.Partners;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.SupplierCodes
{
    /// <summary>
    /// Prepara los códigos de proveedores del formulario: busca a cada proveedor, revisa las reglas del producto
    /// (<see cref="Product.SupplierCodesErrors"/>) y, como necesita la base, que el código no lo use otro producto. Cada
    /// error va con el campo de su fila (decisión 37), así la pantalla lo pone debajo del proveedor o del código.
    /// </summary>
    internal static class ProductSupplierCodeRules
    {
        /// <summary>El proveedor de una fila: <c>supplierCodes[1].supplierId</c>.</summary>
        public static string SupplierField(int row) =>
            FieldName.Item(nameof(CreateProductDto.SupplierCodes), row, nameof(ProductSupplierCodeDto.SupplierId));

        /// <summary>El código de una fila: <c>supplierCodes[1].code</c>.</summary>
        public static string CodeField(int row) =>
            FieldName.Item(nameof(CreateProductDto.SupplierCodes), row, nameof(ProductSupplierCodeDto.Code));

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

            var rows = codes.ToArray();
            var suppliers = (await businessPartnerRepository.GetByIdsAsync(rows.Select(c => c.SupplierId).Distinct().ToArray()))
                .ToDictionary(s => s.Id);

            var missing = Enumerable.Range(0, rows.Length).Where(i => !suppliers.ContainsKey(rows[i].SupplierId)).ToArray();
            if (missing.Length > 0)
                return Failure(
                    missing.Select(i => new ErrorDetail("Este proveedor ya no existe. Quítalo o elige otro.", SupplierField(i))).ToArray(),
                    ErrorType.BadRequest
                    );

            var resolved = rows.Select(c => (suppliers[c.SupplierId], c.Code)).ToArray();

            // El dominio dice de qué proveedor es cada problema; se marca en cada fila de ese proveedor (si está repetido,
            // en las dos).
            var problems = product is null ? Product.NewProductSupplierCodesErrors(resolved) : product.SupplierCodesErrors(resolved);
            if (problems.Count > 0)
                return Failure(
                    problems.SelectMany(p => RowsOf(rows, p.SupplierId)
                        .Select(i => new ErrorDetail(p.Message, p.IsAboutCode ? CodeField(i) : SupplierField(i))))
                        .ToArray(),
                    ErrorType.BadRequest
                    );

            var inUse = await productRepository.SupplierCodesInUseAsync(rows.Select(c => (c.SupplierId, c.Code)).ToArray(), product?.Id);
            if (inUse.Count > 0)
                return Failure(
                    inUse.Select(c => new ErrorDetail(
                        ProductSupplierCode.CodeTakenError(c.Code, suppliers[c.SupplierId].Name, c.ProductCode, c.ProductName, c.ProductIsActive),
                        CodeField(RowsOf(rows, c.SupplierId).First())
                        )).ToArray(),
                    ErrorType.Conflict
                    );

            return Result<IReadOnlyCollection<(BusinessPartner, string)>>.Success(resolved);
        }

        private static IEnumerable<int> RowsOf(ProductSupplierCodeDto[] rows, Guid supplierId) =>
            Enumerable.Range(0, rows.Length).Where(i => rows[i].SupplierId == supplierId);

        private static Result<IReadOnlyCollection<(BusinessPartner, string)>> Failure(IReadOnlyCollection<ErrorDetail> errors, ErrorType type) =>
            Result<IReadOnlyCollection<(BusinessPartner, string)>>.Failure(errors, type);
    }
}
