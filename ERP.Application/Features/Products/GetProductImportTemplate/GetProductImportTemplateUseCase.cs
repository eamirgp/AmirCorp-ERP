using ERP.Application.Contracts.Infrastructure;

namespace ERP.Application.Features.Products.GetProductImportTemplate
{
    internal sealed class GetProductImportTemplateUseCase : IGetProductImportTemplateUseCase
    {
        private readonly IProductSpreadsheet _spreadsheet;

        public GetProductImportTemplateUseCase(IProductSpreadsheet spreadsheet) => _spreadsheet = spreadsheet;

        public Task<byte[]> ExecuteAsync() => Task.FromResult(_spreadsheet.Write([]));
    }
}
