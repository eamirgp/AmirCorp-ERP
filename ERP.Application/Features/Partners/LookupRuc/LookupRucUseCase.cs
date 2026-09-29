using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.LookupRuc
{
    /// <summary>Datos de SUNAT para llenar el formulario, con avisos listos para mostrar.</summary>
    public sealed record LookupRucResponseDto(
        string Ruc,
        string Name,
        string Status,
        string Condition,
        string? Address,
        // Avisos para revisar antes de guardar: contribuyente de baja o no habido, o ya registrado en el sistema.
        IReadOnlyCollection<string> Warnings
        );

    public interface ILookupRucUseCase
    {
        Task<Result<LookupRucResponseDto>> ExecuteAsync(string ruc, CancellationToken ct = default);
    }

    internal sealed class LookupRucUseCase : ILookupRucUseCase
    {
        private const string ActiveStatus = "ACTIVO";
        private const string LocatedCondition = "HABIDO";

        private readonly IRucLookup _rucLookup;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;

        public LookupRucUseCase(IRucLookup rucLookup, IBusinessPartnerRepository businessPartnerRepository)
        {
            _rucLookup = rucLookup;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public async Task<Result<LookupRucResponseDto>> ExecuteAsync(string ruc, CancellationToken ct = default)
        {
            if (!_rucLookup.IsConfigured)
                return Result<LookupRucResponseDto>.Failure(["La consulta de RUC en SUNAT no está configurada. Escribe la razón social a mano."], ErrorType.Unavailable);

            // El RUC se revisa antes de consultar: así no se gasta una consulta en un número mal escrito.
            var normalized = IdentityDocumentTypeExtensions.NormalizeDocumentNumber(ruc);
            if (IdentityDocumentType.Ruc.DocumentNumberError(normalized) is { } error)
                return Result<LookupRucResponseDto>.Failure([error], ErrorType.BadRequest);

            var outcome = await _rucLookup.FindAsync(normalized, ct);

            if (outcome.Data is not { } data)
                return outcome.Failure switch
                {
                    RucLookupFailure.NotFound => Result<LookupRucResponseDto>.Failure(
                        [$"SUNAT no tiene registrado el RUC {normalized}. Revisa que esté bien escrito."], ErrorType.NotFound),
                    RucLookupFailure.Unauthorized => Result<LookupRucResponseDto>.Failure(
                        ["El servicio de consulta de RUC rechazó la clave: puede haber vencido. Avisa al administrador del sistema y, mientras tanto, escribe la razón social a mano."],
                        ErrorType.Unavailable),
                    _ => Result<LookupRucResponseDto>.Failure(
                        ["No se pudo consultar SUNAT en este momento. Inténtalo en unos minutos o escribe la razón social a mano."], ErrorType.Unavailable),
                };

            var warnings = new List<string>();

            if (!data.Status.Equals(ActiveStatus, StringComparison.OrdinalIgnoreCase) || !data.Condition.Equals(LocatedCondition, StringComparison.OrdinalIgnoreCase))
                warnings.Add($"Según SUNAT está {data.Status} y {data.Condition}. Revisa antes de comprarle: sus facturas podrían no servir para el crédito fiscal del IGV.");

            if (await _businessPartnerRepository.FindByDocumentAsync(IdentityDocumentType.Ruc, normalized) is { } existing)
                warnings.Add($"Ya está registrado en el sistema como {existing.Name}.");

            return Result<LookupRucResponseDto>.Success(new LookupRucResponseDto(data.Ruc, data.Name, data.Status, data.Condition, data.Address, warnings));
        }
    }
}
