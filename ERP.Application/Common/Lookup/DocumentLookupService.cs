using ERP.Application.Common.Results;
using ERP.Application.Contracts.Infrastructure;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Common.Lookup
{
    /// <summary>Lo que respondió SUNAT (RUC) o RENIEC (DNI) para un documento ya validado.</summary>
    internal sealed record DocumentLookupData(
        string DocumentNumber,
        string Name,
        // "SUNAT" o "RENIEC".
        string Source,
        // Solo con RUC: estado, condición y dirección.
        RucLookupData? Ruc
        );

    /// <summary>
    /// Consulta un documento en SUNAT o RENIEC con los mensajes de error listos para mostrar. La usan los formularios
    /// de clientes y proveedores y el de empresas; cada uno agrega sus propios avisos.
    /// </summary>
    internal sealed class DocumentLookupService
    {
        private readonly IRucLookup _rucLookup;

        public DocumentLookupService(IRucLookup rucLookup) => _rucLookup = rucLookup;

        public async Task<Result<DocumentLookupData>> FindAsync(IdentityDocumentType identityDocumentType, string? documentNumber, CancellationToken ct)
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

            return failure switch
            {
                null => Result<DocumentLookupData>.Success(new DocumentLookupData(normalized, name, source, ruc)),
                RucLookupFailure.NotFound => Failure(
                    [$"{source} no tiene registrado el {identityDocumentType.Description} {normalized}. Revisa que esté bien escrito."], ErrorType.NotFound),
                RucLookupFailure.Unauthorized => Failure(
                    ["El servicio de consulta rechazó la clave: puede haber vencido. Avisa al administrador del sistema y, mientras tanto, escribe el nombre a mano."],
                    ErrorType.Unavailable),
                RucLookupFailure.QuotaExceeded => Failure(
                    [$"Se acabaron las consultas a {source} de este mes. Escribe el nombre a mano; las consultas vuelven el próximo mes."],
                    ErrorType.Unavailable),
                _ => Failure(
                    [$"No se pudo consultar {source} en este momento. Inténtalo en unos minutos o escribe el nombre a mano."], ErrorType.Unavailable),
            };
        }

        private static Result<DocumentLookupData> Failure(string[] errors, ErrorType type) =>
            Result<DocumentLookupData>.Failure(errors, type);
    }
}
