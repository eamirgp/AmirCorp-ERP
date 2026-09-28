using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Products.UpdateProduct
{
    internal sealed class UpdateProductUseCase : IUpdateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductUseCase(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(UpdateProductDto request)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
                return Result.Failure(["El producto no existe."], ErrorType.NotFound);

            if (await _productRepository.CodeExistsAsync(request.Code, request.Id))
                return Result.Failure(["El código ya se encuentra en uso."], ErrorType.Conflict);

            product.UpdateCode(request.Code);
            product.UpdateName(request.Name);
            product.UpdateUnitOfMeasure(request.UnitOfMeasure);
            product.UpdateIgvAffectation(request.IgvAffectation);
            product.UpdateSalePrice(request.SalePrice);

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
