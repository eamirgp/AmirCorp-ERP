using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.SupplierCodes
{
    /// <summary>
    /// Reglas de los códigos de proveedores que necesitan datos: el proveedor existe y es proveedor, y el código
    /// no lo usa otro producto del mismo proveedor. Los mensajes nombran al proveedor para que se entiendan.
    /// </summary>
    internal static class ProductSupplierCodeRules
    {
        /// <param name="product">El producto que se edita, o null si se está creando.</param>
        public static async Task<Result> CheckAsync(
            IReadOnlyCollection<ProductSupplierCodeDto> codes,
            Product? product,
            IBusinessPartnerRepository businessPartnerRepository,
            IProductRepository productRepository
            )
        {
            if (codes.Count == 0)
                return Result.Success();

            var errors = new List<string>();
            var suppliers = (await businessPartnerRepository.GetByIdsAsync(codes.Select(c => c.SupplierId).Distinct().ToArray()))
                .ToDictionary(s => s.Id);

            foreach (var group in codes.GroupBy(c => c.SupplierId))
            {
                if (!suppliers.TryGetValue(group.Key, out var supplier))
                {
                    errors.Add("Uno de los proveedores elegidos ya no existe. Quítalo y elige otro.");
                    continue;
                }

                if (group.Count() > 1)
                    errors.Add($"{supplier.Name} aparece más de una vez. Deja un solo código por proveedor.");

                if (!supplier.IsSupplier)
                    errors.Add($"{supplier.Name} no está registrado como proveedor.");

                // Un proveedor con compras bloqueadas conserva los códigos que ya tenía, pero no se le agregan nuevos.
                var alreadyLinked = product?.SupplierCodes.Any(c => c.SupplierId == supplier.Id) ?? false;
                if (supplier.IsSupplier && supplier.IsPurchasingBlocked && !alreadyLinked)
                    errors.Add($"Las compras a {supplier.Name} están bloqueadas. Desbloquéalas o elige otro proveedor.");
            }

            if (errors.Count > 0)
                return Result.Failure(errors, ErrorType.BadRequest);

            var inUse = await productRepository.SupplierCodesInUseAsync(codes.Select(c => (c.SupplierId, c.Code)).ToArray(), product?.Id);
            if (inUse.Count > 0)
                return Result.Failure(
                    inUse.Select(c => $"El código {c.Code} de {suppliers[c.SupplierId].Name} ya está en el producto {c.ProductCode} · {c.ProductName}.").ToArray(),
                    ErrorType.Conflict
                    );

            return Result.Success();
        }
    }
}
