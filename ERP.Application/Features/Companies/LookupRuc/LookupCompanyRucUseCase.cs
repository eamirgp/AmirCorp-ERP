using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Companies;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Companies.LookupRuc
{
    /// <summary>Razón social, estado y condición de SUNAT para el formulario de empresas.</summary>
    internal sealed class LookupCompanyRucUseCase : ILookupCompanyRucUseCase
    {
        private readonly DocumentLookupService _documentLookup;
        private readonly ICompanyRepository _companyRepository;

        public LookupCompanyRucUseCase(DocumentLookupService documentLookup, ICompanyRepository companyRepository)
        {
            _documentLookup = documentLookup;
            _companyRepository = companyRepository;
        }

        public async Task<Result<LookupDocumentResponseDto>> ExecuteAsync(string ruc, Guid? companyId = null, CancellationToken ct = default)
        {
            var found = await _documentLookup.FindAsync(IdentityDocumentType.Ruc, ruc, ct);
            if (!found.IsSuccess)
                return Result<LookupDocumentResponseDto>.Failure(found.Errors, found.ErrorType!.Value);

            var data = found.Value!;
            var warnings = new List<string>();

            if (data.Ruc is { } sunat && !TaxpayerStatus.IsActiveAndLocated(sunat.Status, sunat.Condition))
                warnings.Add($"{TaxpayerStatus.Describe(sunat.Status, sunat.Condition)}. Revísalo con tu contador: sus comprobantes podrían no ser válidos.");

            // La razón social llega al formulario tal cual; si no cumple la regla de la empresa, se avisa ya y no al guardar.
            if (Company.NameError(data.Name) is { } nameError)
                warnings.Add($"La razón social que trae {data.Source} no se puede guardar así: {nameError} Corrígela antes de guardar.");

            if (await _companyRepository.FindByRucAsync(data.DocumentNumber, companyId) is { } owner)
                warnings.Add(Company.RucTakenError(owner));

            return Result<LookupDocumentResponseDto>.Success(LookupDocumentResponseDto.From(data, warnings));
        }
    }
}
