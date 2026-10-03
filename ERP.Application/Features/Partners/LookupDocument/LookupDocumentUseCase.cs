using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.LookupDocument
{
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
                // El mismo formulario sirve para clientes y proveedores: el aviso vale para los dos.
                warnings.Add($"{TaxpayerStatus.Describe(ruc.Status, ruc.Condition)}. Revísalo antes de comprarle o venderle: sus comprobantes podrían no ser válidos para SUNAT.");

            // El nombre llega al formulario tal cual; si no cumple la regla del registro, se avisa ya y no al guardar.
            if (BusinessPartner.NameError(data.Name) is { } nameError)
                warnings.Add($"El nombre que trae {data.Source} no se puede guardar así: {nameError} Corrígelo antes de guardar.");

            if (await _businessPartnerRepository.FindByDocumentAsync(identityDocumentType, data.DocumentNumber) is { } existing && existing.Id != partnerId)
                warnings.Add($"Ya está registrado en el sistema como {existing.Name}.");

            return Result<LookupDocumentResponseDto>.Success(LookupDocumentResponseDto.From(data, warnings));
        }
    }
}
