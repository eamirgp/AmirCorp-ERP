using ERP.Application.Common.Lookup;
using ERP.Application.Common.Results;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Companies.LookupRuc
{
    public interface ILookupCompanyRucUseCase
    {
        /// <param name="companyId">La empresa que se está editando, si la hay: tener ese RUC ella misma no es un duplicado.</param>
        Task<Result<LookupDocumentResponseDto>> ExecuteAsync(string ruc, Guid? companyId = null, CancellationToken ct = default);
    }

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
                warnings.Add($"Según SUNAT está {sunat.Status} y {sunat.Condition}. Revísalo con tu contador: sus comprobantes podrían no ser válidos.");

            if (await _companyRepository.RucExistsAsync(data.DocumentNumber, companyId))
                warnings.Add("Ya hay una empresa registrada con este RUC.");

            return Result<LookupDocumentResponseDto>.Success(LookupDocumentResponseDto.From(data, warnings));
        }
    }
}
