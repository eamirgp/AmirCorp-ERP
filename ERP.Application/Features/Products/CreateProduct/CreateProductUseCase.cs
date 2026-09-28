using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.CreateProduct
{
    internal sealed class CreateProductUseCase : ICreateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductUseCase(
            IProductRepository productRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateProductDto request)
        {
            if (await _productRepository.CodeExistsAsync(request.Code))
                return Result<CreatedResponseDto>.Failure(["El código ya se encuentra en uso."], ErrorType.Conflict);

            var product = Product.Create(
                request.Code,
                request.Name,
                request.UnitOfMeasure,
                request.IgvAffectation,
                request.SalePrice
                );

            _productRepository.Add(product);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(product.Id));
        }
    }
}
