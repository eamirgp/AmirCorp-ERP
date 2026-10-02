using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Catalogs;
using ERP.Domain.Inventory;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using ERP.Domain.Products;
using ERP.Application.Features.Partners;
using ERP.Application.Features.UnitsOfMeasure;
using ERP.Domain.Purchases;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    internal sealed class CreatePurchaseUseCase : ICreatePurchaseUseCase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockEntryRepository _stockEntryRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePurchaseUseCase(
            IPurchaseRepository purchaseRepository,
            ICompanyRepository companyRepository,
            IBusinessPartnerRepository businessPartnerRepository,
            IProductRepository productRepository,
            IStockEntryRepository stockEntryRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            IUnitOfWork unitOfWork
            )
        {
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _purchaseRepository = purchaseRepository;
            _companyRepository = companyRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _productRepository = productRepository;
            _stockEntryRepository = stockEntryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreatePurchaseDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company is null)
                return Result<CreatedResponseDto>.Failure(["La empresa no existe."], ErrorType.NotFound);

            if (!company.IsActive)
                return Result<CreatedResponseDto>.Failure(["La empresa está desactivada."], ErrorType.BadRequest);

            BusinessPartner? supplier;

            if (request.NewSupplier is { } newSupplier)
            {
                // Proveedor nuevo: se registra con la compra, en la misma transacción. Si entretanto alguien ya lo
                // registró (o existía solo como cliente), se usa ese registro en vez de duplicarlo.
                var existing = await _businessPartnerRepository.FindByDocumentAsync(IdentityDocumentType.Ruc, newSupplier.Ruc);
                if (existing is null)
                {
                    supplier = BusinessPartner.Create(IdentityDocumentType.Ruc, newSupplier.Ruc, Countries.Peru, newSupplier.Name, isClient: false, isSupplier: true);
                    _businessPartnerRepository.Add(supplier);
                }
                else
                {
                    supplier = await _businessPartnerRepository.GetByIdAsync(existing.Id);
                    if (supplier is { IsSupplier: false })
                        supplier.AddSupplierRole();
                }
            }
            else
                supplier = await _businessPartnerRepository.GetByIdAsync(request.SupplierId!.Value);

            if (supplier is null)
                return Result<CreatedResponseDto>.Failure(["El proveedor no existe."], ErrorType.NotFound);

            if (!supplier.IsSupplier)
                return Result<CreatedResponseDto>.Failure(["El cliente elegido no está registrado como proveedor."], ErrorType.BadRequest);

            // Una empresa no se compra a sí misma: casi siempre es el RUC propio escrito por error en vez del proveedor.
            if (supplier.IdentityDocumentType.IsDomesticTaxpayer && supplier.DocumentNumber == company.Ruc)
                return Result<CreatedResponseDto>.Failure(
                    [$"El proveedor tiene el mismo RUC que la empresa que compra ({company.Ruc}). Revisa cuál de los dos está mal elegido."], ErrorType.BadRequest);

            if (supplier.IsPurchasingBlocked)
                return Result<CreatedResponseDto>.Failure([BusinessPartnerRules.PurchasingBlocked(supplier)], ErrorType.BadRequest);

            if (!supplier.IdentityDocumentType.IsDomesticTaxpayer)
                return Result<CreatedResponseDto>.Failure(["El proveedor de una compra nacional debe tener RUC."], ErrorType.BadRequest);

            var number = Purchase.NormalizeNumber(request.Number);

            if (await _purchaseRepository.DocumentExistsAsync(request.CompanyId, request.TaxDocumentType, supplier.Id, request.Serie, number))
                return Result<CreatedResponseDto>.Failure(["El comprobante ya se encuentra registrado para este proveedor."], ErrorType.Conflict);

            var lines = request.Lines.ToArray();
            var productIds = lines.Where(l => l.ProductId is not null).Select(l => l.ProductId!.Value).ToArray();
            var products = await _productRepository.GetByIdsAsync(productIds);
            var productsDictionary = products.ToDictionary(p => p.Id);

            // Unidades de las líneas y, si hay productos nuevos, la unidad en que se cuenta su stock.
            var unitCodes = lines
                .Select(l => UnitOfMeasure.NormalizeCode(l.InvoiceUnitOfMeasureCode))
                .Append(UnitOfMeasure.BaseUnitCode)
                .Distinct()
                .ToArray();
            var units = (await _unitOfMeasureRepository.GetByCodesAsync(unitCodes)).ToDictionary(u => u.Code);

            var errors = new List<string>();

            for (var i = 0; i < lines.Length; i++)
            {
                var lineNumber = i + 1;

                if (lines[i].ProductId is { } productId)
                {
                    if (!productsDictionary.TryGetValue(productId, out var product))
                        errors.Add($"Línea {lineNumber}: El producto no existe.");
                    else if (!product.IsActive)
                        errors.Add($"Línea {lineNumber}: El producto '{product.Name}' está desactivado.");
                }

                var code = lines[i].InvoiceUnitOfMeasureCode;
                if (UnitOfMeasureRules.CheckUsable(units.GetValueOrDefault(UnitOfMeasure.NormalizeCode(code)), code) is { } unitError)
                    errors.Add($"Línea {lineNumber}: {unitError}");
            }

            errors.AddRange(await CheckNewProductsAsync(lines, supplier, units));

            if (errors.Count > 0)
                return Result<CreatedResponseDto>.Failure(errors, ErrorType.BadRequest);

            // Productos nuevos: se registran con la compra, en la misma transacción. Toman la afectación al IGV de su
            // línea, se cuentan en unidades (lo comprado por caja o docena se convierte con las unidades por caja) y
            // nacen sin precio de venta.
            var lineProducts = new Product[lines.Length];
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].NewProduct is not { } newProduct)
                {
                    lineProducts[i] = productsDictionary[lines[i].ProductId!.Value];
                    continue;
                }

                var created = Product.Create(
                    newProduct.Code,
                    newProduct.Name,
                    UnitOfMeasure.BaseUnitCode,
                    lines[i].InvoiceIgvAffectation,
                    salePrice: 0);

                if (newProduct.SupplierCode is { } supplierCode)
                    created.SetSupplierCodes([(supplier.Id, supplierCode)]);

                _productRepository.Add(created);
                lineProducts[i] = created;
            }

            var purchase = Purchase.Create(
                request.CompanyId,
                request.TaxDocumentType,
                request.Serie,
                number,
                request.IssueDate,
                request.Currency,
                request.ExchangeRate,
                request.InvoicePriceType,
                supplier.Id,
                supplier.IdentityDocumentType,
                supplier.DocumentNumber,
                supplier.Name
                );

            var stockEntries = new List<StockEntry>();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var product = lineProducts[i];

                var purchaseLine = purchase.AddLine(
                    product.Id,
                    product.Code,
                    product.Name,
                    line.InvoiceIgvAffectation,
                    units[UnitOfMeasure.NormalizeCode(line.InvoiceUnitOfMeasureCode)],
                    line.InvoiceQuantity,
                    line.InvoiceAmount,
                    line.ConversionFactor
                    );

                var stockEntry = StockEntry.Create(
                    company.Id,
                    product.Id,
                    purchaseLine.Id,
                    purchaseLine.InventoryQuantity,
                    purchaseLine.InventoryUnitCost,
                    request.IssueDate
                    );

                stockEntries.Add(stockEntry);
            }

            purchase.EnsureHasLines();

            _purchaseRepository.Add(purchase);

            _stockEntryRepository.AddRange(stockEntries);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(purchase.Id));
        }

        /// <summary>
        /// Reglas de los productos nuevos que necesitan datos: el código interno no lo usa otro producto ni otra línea,
        /// el código del proveedor tampoco, y la unidad en que se cuenta su stock está activa.
        /// </summary>
        private async Task<List<string>> CheckNewProductsAsync(CreatePurchaseLineDto[] lines, BusinessPartner supplier, Dictionary<string, UnitOfMeasure> units)
        {
            var errors = new List<string>();
            var news = lines.Select((line, i) => (Line: line, Number: i + 1)).Where(x => x.Line.NewProduct is not null).ToList();
            if (news.Count == 0)
                return errors;

            var codes = news.Select(x => Product.NormalizeCode(x.Line.NewProduct!.Code)).ToArray();
            var taken = (await _productRepository.GetByCodesAsync(codes.Distinct().ToArray())).ToDictionary(p => p.Code);

            var supplierCodes = news
                .Where(x => x.Line.NewProduct!.SupplierCode is not null)
                .Select(x => (supplier.Id, Code: ProductSupplierCode.NormalizeCode(x.Line.NewProduct!.SupplierCode!)))
                .ToArray();
            var supplierCodesTaken = supplierCodes.Length == 0
                ? []
                : (await _productRepository.SupplierCodesInUseAsync(supplierCodes)).ToDictionary(c => c.Code);

            foreach (var (line, number) in news)
            {
                var newProduct = line.NewProduct!;
                var code = Product.NormalizeCode(newProduct.Code);

                if (taken.TryGetValue(code, out var existing))
                    errors.Add($"Línea {number}: El código interno {code} ya es de {existing.Name}. Elígelo de la lista o usa otro código.");
                else if (codes.Count(c => c == code) > 1)
                    errors.Add($"Línea {number}: El código interno {code} está en más de un producto nuevo de esta compra.");

                if (newProduct.SupplierCode is { } supplierCode)
                {
                    var normalized = ProductSupplierCode.NormalizeCode(supplierCode);
                    if (supplierCodesTaken.TryGetValue(normalized, out var used))
                        errors.Add($"Línea {number}: El código {normalized} de {supplier.Name} ya está en el producto {used.ProductCode} · {used.ProductName}. Elígelo de la lista.");
                    else if (supplierCodes.Count(c => c.Code == normalized) > 1)
                        errors.Add($"Línea {number}: El código de proveedor {normalized} está en más de un producto nuevo de esta compra.");
                }

            }

            // Los productos nuevos se cuentan en unidades: esa unidad tiene que estar activa.
            if (UnitOfMeasureRules.CheckUsable(units.GetValueOrDefault(UnitOfMeasure.BaseUnitCode), UnitOfMeasure.BaseUnitCode) is { } unitError)
                errors.Add($"Los productos nuevos se cuentan en unidades: {unitError}");

            return errors;
        }
    }
}
