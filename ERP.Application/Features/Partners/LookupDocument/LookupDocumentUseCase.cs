using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.LookupDocument
{
    public interface ILookupDocumentUseCase
    {
        /// <param name="partnerId">El registro que se está editando, si lo hay: tener ese documento él mismo no es un duplicado.</param>
        Task<Result<LookupDocumentResponseDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber, Guid? partnerId = null, CancellationToken ct = default);
    }

    /// <summary>Datos de SUNAT (RUC) o RENIEC (DNI) para el formulario de clientes y proveedores.</summary>
    internal sealed class LookupDocumentUseCase : ILookupDocumentUseCase
    {
        private readonly DocumentLookupService _documentLookup;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;

        public LookupDocumentUseCase(DocumentLookupService documentLookup, IBusinessPartnerRepository businessPartnerRepository)
        {
            _documentLookup = documentLookup;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public async Task<Result<LookupDocumentResponseDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber, Guid? partnerId = null, CancellationToken ct = default)
        {
            var found = await _documentLookup.FindAsync(identityDocumentType, documentNumber, ct);
            if (!found.IsSuccess)
                return Result<LookupDocumentResponseDto>.Failure(found.Errors, found.ErrorType!.Value);

            var data = found.Value!;
            var warnings = new List<string>();

            if (data.Ruc is { } ruc && !TaxpayerStatus.IsActiveAndLocated(ruc.Status, ruc.Condition))
                warnings.Add($"Según SUNAT está {ruc.Status} y {ruc.Condition}. Revisa antes de comprarle: sus facturas podrían no servir para el crédito fiscal del IGV.");

            if (await _businessPartnerRepository.FindByDocumentAsync(identityDocumentType, data.DocumentNumber) is { } existing && existing.Id != partnerId)
                warnings.Add($"Ya está registrado en el sistema como {existing.Name}.");

            return Result<LookupDocumentResponseDto>.Success(LookupDocumentResponseDto.From(data, warnings));
        }
    }
}
