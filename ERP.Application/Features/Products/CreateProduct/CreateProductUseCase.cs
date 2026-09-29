using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Products;

namespace ERP.Application.Features.Products.CreateProduct
{
    internal sealed class CreateProductUseCase : ICreateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductUseCase(
            IProductRepository productRepository,
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateProductDto request)
        {
            if (await _productRepository.CodeExistsAsync(request.Code))
                return Result<CreatedResponseDto>.Failure(["El código interno ya se encuentra en uso."], ErrorType.Conflict);

            var supplierCodes = await ProductSupplierCodeRules.CheckAsync(request.SupplierCodes, null, _businessPartnerRepository, _productRepository);
            if (!supplierCodes.IsSuccess)
                return Result<CreatedResponseDto>.Failure(supplierCodes.Errors, supplierCodes.ErrorType!.Value);

            var product = Product.Create(
                request.Code,
                request.Name,
                request.UnitOfMeasure,
                request.IgvAffectation,
                request.SalePrice
                );

            product.SetSupplierCodes(request.SupplierCodes.Select(c => (c.SupplierId, c.Code)).ToArray());

            _productRepository.Add(product);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(product.Id));
        }
    }
}
