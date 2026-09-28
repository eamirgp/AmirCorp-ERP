using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Products.DeactivateProduct
{
    public interface IDeactivateProductUseCase : IUseCase<DeactivateProductDto, Result> { }
}
