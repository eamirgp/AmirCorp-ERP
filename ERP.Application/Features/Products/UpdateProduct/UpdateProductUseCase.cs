using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Application.Features.Products.SupplierCodes;

namespace ERP.Application.Features.Products.UpdateProduct
{
    internal sealed class UpdateProductUseCase : IUpdateProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductUseCase(
            IProductRepository productRepository,
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfWork unitOfWork
            )
        {
            _productRepository = productRepository;
            _businessPartnerRepository = businessPartnerRepository;
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

            var supplierCodes = await ProductSupplierCodeRules.CheckAsync(request.SupplierCodes, product, _businessPartnerRepository, _productRepository);
            if (!supplierCodes.IsSuccess)
                return supplierCodes;

            product.UpdateCode(request.Code);
            product.UpdateName(request.Name);
            product.UpdateUnitOfMeasure(request.UnitOfMeasure);
            product.UpdateIgvAffectation(request.IgvAffectation);
            product.UpdateSalePrice(request.SalePrice);
            product.SetSupplierCodes(request.SupplierCodes.Select(c => (c.SupplierId, c.Code)).ToArray());

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
