using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Partners.GetBusinessPartner
{
    internal sealed class GetBusinessPartnerUseCase : IGetBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerQueries _businessPartnerQueries;

        public GetBusinessPartnerUseCase(IBusinessPartnerQueries businessPartnerQueries) => _businessPartnerQueries = businessPartnerQueries;

        public async Task<Result<GetBusinessPartnerResponseDto>> ExecuteAsync(GetBusinessPartnerDto request) =>
            Result<GetBusinessPartnerResponseDto>.FoundOr(await _businessPartnerQueries.GetBusinessPartnerAsync(request), "El cliente o proveedor no existe.");
    }
}
