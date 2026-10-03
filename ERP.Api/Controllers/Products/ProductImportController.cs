using ERP.Api.Controllers.Products.Requests;
using ERP.Api.Extensions;
using ERP.Application.Features.Products.ExportProducts;
using ERP.Application.Features.Products.GetProductImportTemplate;
using ERP.Application.Features.Products.ImportProducts;
using ERP.Application.Features.Products.PreviewProductImport;
using ERP.Application.Features.Products.ProductImport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers.Products
{
    /// <summary>
    /// Carga masiva de productos con Excel: plantilla, exportación, vista previa y confirmación.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ApiController]
    [Route("api/products")]
    public sealed class ProductImportController : ControllerBase
    {
        private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        // El doble del tamaño permitido: así un archivo un poco más grande llega a Validate y recibe "pesa más de 5 MB";
        // uno enorme se corta antes (413) sin leerlo entero.
        private const long MaxRequestSize = ImportProductsRequest.MaxFileSize * 2;

        private readonly IGetProductImportTemplateUseCase _getTemplateUseCase;
        private readonly IExportProductsUseCase _exportProductsUseCase;
        private readonly IPreviewProductImportUseCase _previewProductImportUseCase;
        private readonly IImportProductsUseCase _importProductsUseCase;

        public ProductImportController(
            IGetProductImportTemplateUseCase getTemplateUseCase,
            IExportProductsUseCase exportProductsUseCase,
            IPreviewProductImportUseCase previewProductImportUseCase,
            IImportProductsUseCase importProductsUseCase
            )
        {
            _getTemplateUseCase = getTemplateUseCase;
            _exportProductsUseCase = exportProductsUseCase;
            _previewProductImportUseCase = previewProductImportUseCase;
            _importProductsUseCase = importProductsUseCase;
        }

        /// <summary>Plantilla vacía para la carga masiva.</summary>
        [HttpGet("import/template")]
        [ProducesResponseType<Stream>(StatusCodes.Status200OK, XlsxContentType)]
        public async Task<IActionResult> Template()
        {
            var file = await _getTemplateUseCase.ExecuteAsync();
            return File(file.Content, XlsxContentType, file.FileName);
        }

        /// <summary>
        /// Productos en el formato de la plantilla, para editarlos y volver a subirlos. Acepta los mismos
        /// filtros y orden que la lista: sin filtros exporta todos; con filtros, solo los que coinciden.
        /// 400 si son más de los que admite la carga.
        /// </summary>
        [HttpGet("export")]
        [ProducesResponseType<Stream>(StatusCodes.Status200OK, XlsxContentType)]
        public async Task<IActionResult> Export([FromQuery]ExportProductsRequest exportProductsRequest)
        {
            var result = await _exportProductsUseCase.ExecuteAsync(exportProductsRequest.ToDto());
            if (!result.IsSuccess)
                return result.ToActionResult(StatusCodes.Status200OK);

            return File(result.Value!.Content, XlsxContentType, result.Value.FileName);
        }

        /// <summary>Lee la planilla y devuelve qué pasará con cada fila. No guarda nada.</summary>
        [HttpPost("import/preview")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxRequestSize)]
        [ProducesResponseType<ProductImportPreviewDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Preview([FromForm] ImportProductsRequest request)
        {
            var errors = request.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            await using var file = await CopyAsync(request.File!);
            var result = await _previewProductImportUseCase.ExecuteAsync(request.ToDto(file));
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Confirma la carga masiva: vuelve a validar el archivo y guarda todo en una sola transacción.
        /// Si una fila tiene errores no se guarda ninguna.
        /// </summary>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxRequestSize)]
        [ProducesResponseType<ProductImportResultDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Import([FromForm] ImportProductsRequest request)
        {
            var errors = request.Validate();
            if (errors.Count > 0)
                return errors.ToBadRequest();

            await using var file = await CopyAsync(request.File!);
            var result = await _importProductsUseCase.ExecuteAsync(request.ToDto(file));
            return result.ToActionResult(StatusCodes.Status200OK);
        }

        /// <summary>Copia el archivo a memoria: la librería de Excel necesita poder recorrerlo libremente.</summary>
        private static async Task<MemoryStream> CopyAsync(IFormFile file)
        {
            var memory = new MemoryStream();
            await file.CopyToAsync(memory);
            memory.Position = 0;
            return memory;
        }
    }
}
