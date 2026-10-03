using ERP.Application.Features.Purchases.CreatePurchase;
using ERP.Domain.Catalogs;
using ERP.Domain.Products;

namespace ERP.Api.Controllers.Purchases.Requests
{
    public sealed record CreatePurchaseLineRequest(
        Guid? ProductId,
        // Producto que todavía no existe: se registra junto con la compra. Va en vez de ProductId.
        CreatePurchaseNewProductRequest? NewProduct,
        IgvAffectation? InvoiceIgvAffectation,
        string? InvoiceUnitOfMeasureCode,
        decimal? InvoiceQuantity,
        decimal? InvoiceAmount,
        decimal? ConversionFactor
        )
    {
        public IReadOnlyCollection<string> Validate(int lineNumber, InvoicePriceType? invoicePriceType)
        {
            var errors = new List<string>();

            var amountLabel = invoicePriceType is InvoicePriceType.UnitPrice
                ? "El precio unitario"
                : "El valor unitario";

            // El producto: uno registrado o uno nuevo, no los dos. El nuevo se valida como en su propia pantalla.
            if (NewProduct is not null && ProductId is not null)
                errors.Add($"Línea {lineNumber}: Elige un producto registrado o indica uno nuevo, no los dos.");
            else if (NewProduct is not null)
            {
                if (string.IsNullOrWhiteSpace(NewProduct.Code))
                    errors.Add($"Línea {lineNumber}: El código interno del producto nuevo es requerido.");
                else if (NewProduct.Code.Trim().Length > Product.CodeMaxLength)
                    errors.Add($"Línea {lineNumber}: El código interno no puede exceder los {Product.CodeMaxLength} caracteres.");

                if (string.IsNullOrWhiteSpace(NewProduct.Name))
                    errors.Add($"Línea {lineNumber}: El nombre del producto nuevo es requerido.");
                else if (NewProduct.Name.Trim().Length > Product.NameMaxLength)
                    errors.Add($"Línea {lineNumber}: El nombre no puede exceder los {Product.NameMaxLength} caracteres.");

                if (!string.IsNullOrWhiteSpace(NewProduct.SupplierCode) && NewProduct.SupplierCode.Trim().Length > ProductSupplierCode.CodeMaxLength)
                    errors.Add($"Línea {lineNumber}: El código de proveedor no puede exceder los {ProductSupplierCode.CodeMaxLength} caracteres.");
            }
            else if (ProductId is null || ProductId == Guid.Empty)
                errors.Add($"Línea {lineNumber}: El producto es requerido.");

            if (InvoiceIgvAffectation is null)
                errors.Add($"Línea {lineNumber}: El tipo de afectación del IGV es requerido.");

            if (InvoiceIgvAffectation is not null && !Enum.IsDefined(InvoiceIgvAffectation.Value))
                errors.Add($"Línea {lineNumber}: El tipo de afectación del IGV es inválido.");

            if (string.IsNullOrWhiteSpace(InvoiceUnitOfMeasureCode))
                errors.Add($"Línea {lineNumber}: La unidad de medida es requerida.");

            if (InvoiceQuantity is null)
                errors.Add($"Línea {lineNumber}: La cantidad es requerida.");

            if (InvoiceQuantity is not null && InvoiceQuantity <= 0)
                errors.Add($"Línea {lineNumber}: La cantidad debe ser mayor a cero.");

            if (InvoiceAmount is null)
                errors.Add($"Línea {lineNumber}: {amountLabel} es requerido.");

            if (InvoiceAmount is not null && InvoiceAmount <= 0)
                errors.Add($"Línea {lineNumber}: {amountLabel} debe ser mayor a cero.");

            // Las unidades por caja solo hacen falta con unidades variables (Caja): eso lo revisa el registro con el
            // catálogo de unidades. Aquí solo se descarta un valor imposible.
            if (ConversionFactor is not null && ConversionFactor <= 0)
                errors.Add($"Línea {lineNumber}: Las unidades por caja o paquete deben ser mayores a cero.");

            return errors;
        }

        private static string? NullIfBlank(string? text) =>
            string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        public CreatePurchaseLineDto ToDto() =>
            new(
                ProductId,
                NewProduct is null
                    ? null
                    : new CreatePurchaseNewProductDto(NewProduct.Code!.Trim(), NewProduct.Name!.Trim(), NullIfBlank(NewProduct.SupplierCode)),
                InvoiceIgvAffectation!.Value,
                InvoiceUnitOfMeasureCode!,
                InvoiceQuantity!.Value,
                InvoiceAmount!.Value,
                ConversionFactor
                );
    }

    /// <param name="Code">Código interno (la pantalla propone el de la factura).</param>
    /// <param name="SupplierCode">Código con que lo vende el proveedor de esta compra; opcional.</param>
    public sealed record CreatePurchaseNewProductRequest(string? Code, string? Name, string? SupplierCode);
}
