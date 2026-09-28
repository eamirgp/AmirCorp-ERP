using ERP.Application.Features.Products.ProductImport;

namespace ERP.Api.Controllers.Products.Requests
{
    /// <summary>Planilla de productos (.xlsx) y modo de importación.</summary>
    public sealed class ImportProductsRequest
    {
        public const long MaxFileSize = 5 * 1024 * 1024;

        /// <summary>Planilla descargada desde GET api/products/import/template o api/products/export.</summary>
        public IFormFile? File { get; init; }

        /// <summary>Si es true, los productos cuyo código ya existe se actualizan; si no, se omiten.</summary>
        public bool UpdateExisting { get; init; }

        public IReadOnlyCollection<string> Validate()
        {
            var errors = new List<string>();

            if (File is null || File.Length == 0)
                errors.Add("Sube el archivo de Excel con los productos.");
            else if (!Path.GetExtension(File.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                errors.Add("El archivo debe ser un Excel (.xlsx). Descarga la plantilla y usa ese formato.");
            else if (File.Length > MaxFileSize)
                errors.Add("El archivo pesa más de 5 MB. Divídelo en varios archivos.");

            return errors;
        }

        public ImportProductsDto ToDto(Stream file) => new(file, UpdateExisting);
    }
}
