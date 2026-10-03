using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Products.FindProductByCode
{
    public interface IFindProductByCodeUseCase : IQueryUseCase<string, Result<FoundProductDto>> { }
}
