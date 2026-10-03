using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.SupplierCodes;
using ERP.Domain.Products;
using ERP.Domain.UnitsOfMeasure;

namespace ERP.Application.Features.Products.CreateProduct
{
    internal sealed class CreateProductUseCase : ICreateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductUseCase(
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

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateProductDto request)
        {
            // Todos los errores juntos: el código repetido, la unidad y los códigos de proveedores.
            var errors = new List<string>();
            var owner = await _productRepository.FindByCodeAsync(request.Code);
            var codeTaken = owner is not null;
            if (owner is not null)
                errors.Add(Product.CodeTakenError(owner));

            // La misma regla del dominio, revisada antes para responder con el mensaje en vez de una excepción. Sin una
            // unidad que sirva no se puede armar el producto para revisar lo demás.
            var unit = await _unitOfMeasureRepository.GetByCodeAsync(request.UnitOfMeasureCode);
            if (UnitOfMeasure.UsableError(unit, request.UnitOfMeasureCode) is { } unitError)
                return Result<CreatedResponseDto>.Failure([.. errors, unitError], ErrorType.BadRequest);

            var product = Product.Create(
                request.Code,
                request.Name,
                unit!,
                request.IgvAffectation,
                request.SalePrice
                );

            var supplierCodes = await ProductSupplierCodeRules.CheckAsync(request.SupplierCodes, product, _businessPartnerRepository, _productRepository);
            if (!supplierCodes.IsSuccess)
                errors.AddRange(supplierCodes.Errors);

            if (errors.Count > 0)
                return Result<CreatedResponseDto>.Failure(errors, codeTaken ? ErrorType.Conflict : supplierCodes.ErrorType!.Value);

            product.SetSupplierCodes(supplierCodes.Value!);

            _productRepository.Add(product);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(product.Id));
        }
    }
}
