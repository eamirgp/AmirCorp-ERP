using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Products.CreateProduct
{
    public interface ICreateProductUseCase : IUseCase<CreateProductDto, Result<CreatedResponseDto>> { }
}
