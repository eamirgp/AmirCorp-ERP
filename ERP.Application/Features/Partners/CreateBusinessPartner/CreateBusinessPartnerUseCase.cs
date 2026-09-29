using ERP.Application.Common.Responses;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;

namespace ERP.Application.Features.Partners.CreateBusinessPartner
{
    internal sealed class CreateBusinessPartnerUseCase : ICreateBusinessPartnerUseCase
    {
        private readonly IBusinessPartnerRepository _businessPartnerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateBusinessPartnerUseCase(
            IBusinessPartnerRepository businessPartnerRepository,
            IUnitOfWork unitOfWork
            )
        {
            _businessPartnerRepository = businessPartnerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatedResponseDto>> ExecuteAsync(CreateBusinessPartnerDto request)
        {
            if (await _businessPartnerRepository.FindByDocumentAsync(request.IdentityDocumentType, request.DocumentNumber) is { } existing)
                return Result<CreatedResponseDto>.Failure([BusinessPartnerRules.AlreadyExists(existing)], ErrorType.Conflict);

            var businessPartner = BusinessPartner.Create(
                request.IdentityDocumentType,
                request.DocumentNumber,
                request.CountryCode,
                request.Name,
                request.IsClient,
                request.IsSupplier
                );

            _businessPartnerRepository.Add(businessPartner);

            await _unitOfWork.SaveChangesAsync();

            return Result<CreatedResponseDto>.Success(new CreatedResponseDto(businessPartner.Id));
        }
    }
}
