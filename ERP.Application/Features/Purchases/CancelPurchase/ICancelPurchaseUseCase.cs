using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Purchases.CancelPurchase
{
    public interface ICancelPurchaseUseCase : IUseCase<CancelPurchaseDto, Result> { }
}
