using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;

namespace ERP.Application.Features.Products.ActivateProduct
{
    internal sealed class ActivateProductUseCase : IActivateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateProductUseCase(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> ExecuteAsync(ActivateProductDto request)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
                return Result.Failure(["El producto no existe."], ErrorType.NotFound);

            product.Activate();

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
