using ERP.Application.Common.Results;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.Features.Purchases.GetPurchase
{
    public interface IGetPurchaseUseCase : IQueryUseCase<GetPurchaseDto, Result<GetPurchaseResponseDto>> { }
}
