using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Products.UpdateProduct
{
    internal sealed class UpdateProductUseCase : IUpdateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductUseCase(
            IProductRepository productRepository,
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfMeasureRepository unitOfMeasureRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfMeasureRepository = unitOfMeasureRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateProductDto request)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
                return Result.Failure(["El producto no existe."], ErrorType.NotFound);

            // Si alguien lo modificó (otra persona o una importación) después de abrir el formulario, no se pisa su cambio.
            if (_productRepository.VersionOf(product) != request.RowVersion)
                return Result.Failure(
                    ["Otra persona modificó este producto mientras lo editabas. Cierra el formulario y vuelve a abrirlo para ver los datos actuales."],
                    ErrorType.Conflict
                    );

            if (await _productRepository.CodeExistsAsync(request.Code, request.Id))
                return Result.Failure(["El código interno ya se encuentra en uso."], ErrorType.Conflict);

            // Las mismas reglas del dominio, revisadas antes para responder con el mensaje en vez de una excepción. Si no
            // cambia la unidad, se acepta aunque ya no esté activa: el producto puede seguir editándose.
            var unit = await _unitOfMeasureRepository.GetByCodeAsync(request.UnitOfMeasureCode);
            var unitError = unit is null
                ? UnitOfMeasure.UsableError(null, request.UnitOfMeasureCode)
                : product.UnitOfMeasureChangeError(unit);
            if (unitError is not null)
                return Result.Failure([unitError], ErrorType.BadRequest);

            var supplierCodes = await ProductSupplierCodeRules.CheckAsync(request.SupplierCodes, product, _businessPartnerRepository, _productRepository);
            if (!supplierCodes.IsSuccess)
                return Result.Failure(supplierCodes.Errors, supplierCodes.ErrorType!.Value);

            product.UpdateCode(request.Code);
            product.UpdateName(request.Name);
            product.UpdateUnitOfMeasure(unit!);
            product.UpdateIgvAffectation(request.IgvAffectation);
            product.UpdateSalePrice(request.SalePrice);
            product.SetSupplierCodes(supplierCodes.Value!);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
