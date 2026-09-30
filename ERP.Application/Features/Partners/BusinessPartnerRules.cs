using ERP.Application.Common.Formatting;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners
{
    /// <summary>Mensajes de las reglas de clientes y proveedores que necesitan datos de otros registros.</summary>
    internal static class BusinessPartnerRules
    {
        /// <summary>"Ya existe ACME S.A.C. con RUC 20123456789."</summary>
        public static string AlreadyExists(BusinessPartner existing) =>
            $"Ya existe {existing.Name} con {existing.IdentityDocumentType.Description} {existing.DocumentNumber}.";

        /// <summary>
        /// El documento identifica al contribuyente: si ya tiene compras, cambiarlo mezclaría dos empresas en el mismo
        /// registro. Cada compra guarda su propia copia del RUC, así que el historial no se pierde.
        /// </summary>
        public static string DocumentLocked(BusinessPartner partner, int purchases) =>
            $"{partner.Name} ya tiene {Count(purchases, "compra", "compras")} con {partner.IdentityDocumentType.Description} {partner.DocumentNumber}, " +
            "así que el documento no se puede cambiar. Si es otra empresa, regístrala como un cliente o proveedor nuevo.";

        /// <summary>"Las compras a ACME S.A.C. están bloqueadas (motivo: …)."</summary>
        public static string PurchasingBlocked(BusinessPartner supplier) =>
            $"Las compras a {supplier.Name} están bloqueadas" +
            (supplier.PurchasingBlockReason is { } reason ? $" (motivo: {reason})." : ".");

        /// <summary>"Cliente", "Proveedor" o "Cliente y proveedor", para las tablas.</summary>
        public static string RoleDescription(bool isClient, bool isSupplier) => (isClient, isSupplier) switch
        {
            (true, true) => "Cliente y proveedor",
            (true, false) => "Cliente",
            (false, true) => "Proveedor",
            _ => "—"
        };

        public static bool DocumentChanges(BusinessPartner partner, IdentityDocumentType identityDocumentType, string documentNumber) =>
            partner.IdentityDocumentType != identityDocumentType ||
            partner.DocumentNumber != IdentityDocumentTypeExtensions.NormalizeDocumentNumber(documentNumber);

        private static string Count(int n, string singular, string plural) =>
            n == 1 ? $"1 {singular}" : $"{NumberText.Integer(n)} {plural}";
    }
}
