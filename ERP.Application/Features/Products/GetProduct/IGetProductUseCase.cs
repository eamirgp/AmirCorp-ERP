using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Products.GetProduct
{
    public interface IGetProductUseCase : IQueryUseCase<GetProductDto, GetProductResponseDto?> { }
}
