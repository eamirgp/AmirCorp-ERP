using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.SupplierCodes;
using ERP.Application.Features.UnitsOfMeasure;
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

            // Si no cambia la unidad, se acepta aunque ya no esté activa: el producto puede seguir editándose.
            if (UnitOfMeasure.NormalizeCode(request.UnitOfMeasureCode) != product.UnitOfMeasureCode)
            {
                var unitError = UnitOfMeasureRules.CheckUsable(await _unitOfMeasureRepository.GetByCodeAsync(request.UnitOfMeasureCode), request.UnitOfMeasureCode);
                if (unitError is not null)
                    return Result.Failure([unitError], ErrorType.BadRequest);
            }

            var supplierCodes = await ProductSupplierCodeRules.CheckAsync(request.SupplierCodes, product, _businessPartnerRepository, _productRepository);
            if (!supplierCodes.IsSuccess)
                return supplierCodes;

            product.UpdateCode(request.Code);
            product.UpdateName(request.Name);
            product.UpdateUnitOfMeasure(request.UnitOfMeasureCode);
            product.UpdateIgvAffectation(request.IgvAffectation);
            product.UpdateSalePrice(request.SalePrice);
            product.SetSupplierCodes(request.SupplierCodes.Select(c => (c.SupplierId, c.Code)).ToArray());

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
