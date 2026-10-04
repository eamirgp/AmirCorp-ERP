using ERP.Application.Common.Results;
using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Products.Requests
{
    public sealed record ProductSupplierCodeRequest(
        Guid? SupplierId,
        string? Code
        )
    {
        /// <summary>
        /// Errores de la lista de códigos, cada uno con el campo de su fila (<c>supplierCodes[0].supplierId</c>, contando
        /// desde 0; decisión 37): la pantalla lo pone debajo del proveedor o del código de esa fila. Las filas que la
        /// pantalla deja vacías no se envían.
        /// </summary>
        public static IReadOnlyCollection<ErrorDetail> Validate(IReadOnlyCollection<ProductSupplierCodeRequest?>? codes)
        {
            var errors = new List<ErrorDetail>();
            var row = 0;

            foreach (var code in codes ?? [])
            {
                if (code?.SupplierId is null || code.SupplierId == Guid.Empty)
                    errors.Add(new("Elige el proveedor.", Field(row, nameof(SupplierId))));

                if (ProductSupplierCode.CodeError(code?.Code) is { } codeError)
                    errors.Add(new(codeError, Field(row, nameof(Code))));

                row++;
            }

            return errors;
        }

        private static string Field(int row, string property) =>
            FieldName.Item(nameof(CreateProductRequest.SupplierCodes), row, property);

        public static IReadOnlyCollection<ProductSupplierCodeDto> ToDtos(IReadOnlyCollection<ProductSupplierCodeRequest?>? codes) =>
            (codes ?? []).Select(c => new ProductSupplierCodeDto(c!.SupplierId!.Value, c.Code!)).ToArray();
    }
}
