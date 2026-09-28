using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Products.DeactivateProduct
{
    internal sealed class DeactivateProductUseCase : IDeactivateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateProductUseCase(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(DeactivateProductDto request)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
                return Result.Failure(["El producto no existe."], ErrorType.NotFound);

            product.Deactivate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
