using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    public sealed record ProductSupplierCodeRequest(
        Guid? SupplierId,
        string? Code
        )
    {
        /// <summary>Errores de la lista de códigos. Cada mensaje dice en qué fila está, contando desde 1.</summary>
        public static IReadOnlyCollection<string> Validate(IReadOnlyCollection<ProductSupplierCodeRequest?>? codes)
        {
            var errors = new List<string>();
            var row = 0;

            foreach (var code in codes ?? [])
            {
                row++;

                if (code?.SupplierId is null || code.SupplierId == Guid.Empty)
                    errors.Add($"Códigos de proveedores, fila {row}: elige el proveedor.");

                if (ProductSupplierCode.CodeError(code?.Code) is { } codeError)
                    errors.Add($"Códigos de proveedores, fila {row}: {codeError}");
            }

            return errors;
        }

        public static IReadOnlyCollection<ProductSupplierCodeDto> ToDtos(IReadOnlyCollection<ProductSupplierCodeRequest?>? codes) =>
            (codes ?? []).Select(c => new ProductSupplierCodeDto(c!.SupplierId!.Value, c.Code!)).ToArray();
    }
}
