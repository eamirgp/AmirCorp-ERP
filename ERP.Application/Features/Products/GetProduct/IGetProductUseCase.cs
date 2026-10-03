using ERP.Application.Common.Results;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Products.GetProduct
{
    public interface IGetProductUseCase : IQueryUseCase<GetProductDto, Result<GetProductResponseDto>> { }
}
