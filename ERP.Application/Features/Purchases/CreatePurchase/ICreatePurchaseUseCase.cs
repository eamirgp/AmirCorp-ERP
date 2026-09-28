using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Purchases.CreatePurchase
{
    public interface ICreatePurchaseUseCase : IUseCase<CreatePurchaseDto, Result<CreatedResponseDto>> { }
}
