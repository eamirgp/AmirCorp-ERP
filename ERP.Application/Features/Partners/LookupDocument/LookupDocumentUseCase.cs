using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Application.Contracts.Persistence.Commands;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners.LookupDocument
{
    /// <summary>Datos de SUNAT (RUC) o RENIEC (DNI) para llenar el formulario, con avisos listos para mostrar.</summary>
    public sealed record LookupDocumentResponseDto(
        string DocumentNumber,
        string Name,
        // De dónde vienen los datos: "SUNAT" o "RENIEC".
        string Source,
        // Solo SUNAT los informa; con DNI vienen vacíos.
        string? Status,
        string? Condition,
        string? Address,
        // Avisos para revisar antes de guardar: contribuyente de baja o no habido, o ya registrado en el sistema.
        IReadOnlyCollection<string> Warnings
        );

    public interface ILookupDocumentUseCase
    {
        Task<Result<LookupDocumentResponseDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber, CancellationToken ct = default);
    }

    internal sealed class LookupDocumentUseCase : ILookupDocumentUseCase
    {
        private const string ActiveStatus = "ACTIVO";
        private const string LocatedCondition = "HABIDO";

        private readonly IRucLookup _rucLookup;
        private readonly IBusinessPartnerRepository _businessPartnerRepository;

        public LookupDocumentUseCase(IRucLookup rucLookup, IBusinessPartnerRepository businessPartnerRepository)
        {
            _rucLookup = rucLookup;
            _businessPartnerRepository = businessPartnerRepository;
        }

        public async Task<Result<LookupDocumentResponseDto>> ExecuteAsync(IdentityDocumentType identityDocumentType, string documentNumber, CancellationToken ct = default)
        {
            if (identityDocumentType.LookupSource is not { } source)
                return Failure(["Este tipo de documento no se puede consultar. Escribe el nombre a mano."], ErrorType.BadRequest);

            if (!_rucLookup.IsConfigured)
                return Failure([$"La consulta en {source} no está configurada. Escribe el nombre a mano."], ErrorType.Unavailable);

            // El número se revisa antes de consultar: así no se gasta una consulta en uno mal escrito.
            var normalized = IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber ?? "");
            if (identityDocumentType.DocumentNumberError(normalized) is { } error)
                return Failure([error], ErrorType.BadRequest);

            string name;
            RucLookupData? ruc = null;
            RucLookupFailure? failure;

            if (identityDocumentType == IdentityDocumentType.Ruc)
            {
                var outcome = await _rucLookup.FindAsync(normalized, ct);
                (ruc, failure, name) = (outcome.Data, outcome.Failure, outcome.Data?.Name ?? "");
            }
            else
            {
                var outcome = await _rucLookup.FindDniAsync(normalized, ct);
                (failure, name) = (outcome.Failure, outcome.Name ?? "");
            }

            if (failure is not null)
                return failure switch
                {
                    RucLookupFailure.NotFound => Failure(
                        [$"{source} no tiene registrado el {identityDocumentType.Description} {normalized}. Revisa que esté bien escrito."], ErrorType.NotFound),
                    RucLookupFailure.Unauthorized => Failure(
                        ["El servicio de consulta rechazó la clave: puede haber vencido. Avisa al administrador del sistema y, mientras tanto, escribe el nombre a mano."],
                        ErrorType.Unavailable),
                    _ => Failure(
                        [$"No se pudo consultar {source} en este momento. Inténtalo en unos minutos o escribe el nombre a mano."], ErrorType.Unavailable),
                };

            var warnings = new List<string>();

            if (ruc is not null && (!ruc.Status.Equals(ActiveStatus, StringComparison.OrdinalIgnoreCase) || !ruc.Condition.Equals(LocatedCondition, StringComparison.OrdinalIgnoreCase)))
                warnings.Add($"Según SUNAT está {ruc.Status} y {ruc.Condition}. Revisa antes de comprarle: sus facturas podrían no servir para el crédito fiscal del IGV.");

            if (await _businessPartnerRepository.FindByDocumentAsync(identityDocumentType, normalized) is { } existing)
                warnings.Add($"Ya está registrado en el sistema como {existing.Name}.");

            return Result<LookupDocumentResponseDto>.Success(
                new LookupDocumentResponseDto(normalized, name, source, ruc?.Status, ruc?.Condition, ruc?.Address, warnings));
        }

        private static Result<LookupDocumentResponseDto> Failure(string[] errors, ErrorType type) =>
            Result<LookupDocumentResponseDto>.Failure(errors, type);
    }
}
