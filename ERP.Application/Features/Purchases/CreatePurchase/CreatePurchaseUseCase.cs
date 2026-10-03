using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Common;
using ERP.Domain.Inventory;
using ERP.Domain.Purchases;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    internal sealed class CreatePurchaseUseCase : ICreatePurchaseUseCase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockEntryRepository _stockEntryRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly PurchaseSupplierResolver _supplierResolver;
        private readonly PurchaseLinesChecker _linesChecker;
        private readonly PurchaseLineProducts _lineProducts;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public CreatePurchaseUseCase(
            IPurchaseRepository purchaseRepository,
            ICompanyRepository companyRepository,
            IProductRepository productRepository,
            IStockEntryRepository stockEntryRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            PurchaseSupplierResolver supplierResolver,
            PurchaseLinesChecker linesChecker,
            PurchaseLineProducts lineProducts,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider
            )
        {
            _purchaseRepository = purchaseRepository;
            _companyRepository = companyRepository;
            _productRepository = productRepository;
            _stockEntryRepository = stockEntryRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _supplierResolver = supplierResolver;
            _linesChecker = linesChecker;
            _lineProducts = lineProducts;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreatePurchaseDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company is null)
                return Result<CreatedResponseDto>.Failure(["La empresa no existe."], ErrorType.NotFound);

            var supplier = await _supplierResolver.ResolveAsync(request);
            if (supplier is null)
                return Result<CreatedResponseDto>.Failure(["El proveedor no existe."], ErrorType.NotFound);

            // Las reglas del dominio, revisadas antes para avisar todos los errores juntos en vez de lanzar una excepción.
            var errors = new List<string>();
            if (Purchase.PartiesError(company, supplier) is { } partiesError)
                errors.Add(partiesError);

            var number = Purchase.NormalizeNumber(request.Number);

            var registered = await _purchaseRepository.FindDocumentAsync(request.TaxDocumentType, supplier.Id, request.Serie, number);
            if (registered is not null)
                errors.Add(Purchase.DuplicateDocumentError(company.Id, registered.CompanyId, registered.CompanyName));

            var lines = request.Lines.ToArray();
            var productIds = lines.Where(l => l.ProductId is not null).Select(l => l.ProductId!.Value).ToArray();
            var products = (await _productRepository.GetByIdsAsync(productIds)).ToDictionary(p => p.Id);

            // Unidades de las líneas y, si hay productos nuevos, la unidad en que se cuenta su stock.
            var unitCodes = lines
                .Select(l => UnitOfMeasure.NormalizeCode(l.InvoiceUnitOfMeasureCode))
                .Append(UnitOfMeasure.BaseUnitCode)
                .Distinct()
                .ToArray();
            var units = (await _unitOfMeasureRepository.GetByCodesAsync(unitCodes)).ToDictionary(u => u.Code);

            errors.AddRange(await _linesChecker.CheckAsync(lines, request.InvoicePriceType, supplier, products, units));
            if (errors.Count > 0)
                // Solo el comprobante ya registrado es un conflicto con lo guardado; con otros errores, la compra está mal llenada.
                return Result<CreatedResponseDto>.Failure(errors, registered is not null && errors.Count == 1 ? ErrorType.Conflict : ErrorType.BadRequest);

            var lineProducts = _lineProducts.Resolve(lines, supplier, products, units);

            var purchase = Purchase.Create(
                company,
                supplier,
                request.TaxDocumentType,
                request.Serie,
                number,
                request.IssueDate,
                request.Currency,
                request.ExchangeRate,
                request.InvoicePriceType,
                PeruCalendar.Today(_timeProvider.GetUtcNow().UtcDateTime)
                );

            var stockEntries = new List<StockEntry>();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                var purchaseLine = purchase.AddLine(
                    lineProducts[i],
                    line.InvoiceIgvAffectation,
                    units[UnitOfMeasure.NormalizeCode(line.InvoiceUnitOfMeasureCode)],
                    line.InvoiceQuantity,
                    line.InvoiceAmount,
                    line.ConversionFactor
                    );

                stockEntries.Add(StockEntry.FromPurchaseLine(purchase, purchaseLine));
            }

            purchase.EnsureHasLines();

            _purchaseRepository.Add(purchase);
            _stockEntryRepository.AddRange(stockEntries);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(purchase.Id));
        }
    }
}
