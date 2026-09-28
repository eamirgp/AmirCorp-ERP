using ERP.Application.Contracts.Persistence.Queries;

namespace ERP.Application.Features.Partners.GetBusinessPartner
{
    internal sealed class GetBusinessPartnerUseCase : IGetBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerQueries _businessPartnerQueries;

        public GetBusinessPartnerUseCase(IBusinessPartnerQueries businessPartnerQueries) => _businessPartnerQueries = businessPartnerQueries;

        public async Task<GetBusinessPartnerResponseDto?> ExecuteAsync(GetBusinessPartnerDto request) =>
            await _businessPartnerQueries.GetBusinessPartnerAsync(request);
    }
}
