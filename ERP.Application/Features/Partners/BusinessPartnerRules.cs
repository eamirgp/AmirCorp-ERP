using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;

namespace ERP.Application.Features.Partners
{
    /// <summary>
    /// Textos de clientes y proveedores que no son reglas del registro: el aviso de duplicado (necesita buscar en la
    /// base) y la descripción del rol para las tablas. Las reglas están en <see cref="BusinessPartner"/>.
    /// </summary>
    internal static class BusinessPartnerRules
    {
        /// <summary>"Ya existe ACME S.A.C. con RUC 20123456789."</summary>
        public static string AlreadyExists(BusinessPartner existing) =>
            $"Ya existe {existing.Name} con {existing.IdentityDocumentType.Description} {existing.DocumentNumber}.";

        /// <summary>"Cliente", "Proveedor" o "Cliente y proveedor", para las tablas.</summary>
        public static string RoleDescription(bool isClient, bool isSupplier) => (isClient, isSupplier) switch
        {
            (true, true) => "Cliente y proveedor",
            (true, false) => "Cliente",
            (false, true) => "Proveedor",
            _ => "—"
        };
    }
}
