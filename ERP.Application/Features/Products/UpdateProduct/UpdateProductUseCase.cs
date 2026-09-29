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

            // Si alguien lo modificó (otra persona o una importación) después de abrir el formulario, no se pisa su cambio.
            if (_productRepository.VersionOf(product) != request.RowVersion)
                return Result.Failure(
                    ["Otra persona modificó este producto mientras lo editabas. Cierra el formulario y vuelve a abrirlo para ver los datos actuales."],
                    ErrorType.Conflict
                    );

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
