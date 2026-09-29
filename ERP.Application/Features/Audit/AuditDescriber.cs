using ERP.Application.Common.Formatting;
using ERP.Domain.Catalogs;
using ERP.Domain.Companies;
using ERP.Domain.Partners;
using ERP.Domain.Partners.Enums;
using ERP.Domain.Products;
using ERP.Domain.Purchases;
using ERP.Domain.UnitsOfMeasure;
using ERP.Domain.Users;
using ERP.Domain.Users.Enums;

namespace ERP.Application.Features.Audit
{
    /// <summary>
    /// Reglas del historial: qué registros se auditan, cómo se llaman, qué campos se registran y cómo se
    /// muestran sus valores. El historial guarda los textos listos para leer, así el frontend no traduce nada.
    /// </summary>
    public static class AuditDescriber
    {
        public const string IsActiveProperty = "IsActive";
        public const string IsCancelledProperty = "IsCancelled";
        public const string PasswordProperty = "PasswordHash";
        /// <summary>El producto guarda el código de la unidad; el historial muestra su nombre ("NIU" → "Unidad").</summary>
        public const string UnitOfMeasureCodeProperty = nameof(Product.UnitOfMeasureCode);


        // Map: para campos que guardan un código y se muestran con su nombre (el país "CN" → "China").
        private sealed record Field(string Label, bool IsMoney = false, Func<string, string>? Map = null);

        // Solo se registran los campos que el usuario puede cambiar. Los de auditoría, los totales y la
        // versión de concurrencia quedan fuera. La contraseña se registra como acción, nunca su valor.
        private static readonly IReadOnlyDictionary<AuditEntityType, IReadOnlyDictionary<string, Field>> Fields =
            new Dictionary<AuditEntityType, IReadOnlyDictionary<string, Field>>
            {
                [AuditEntityType.Product] = new Dictionary<string, Field>
                {
                    [nameof(Product.Code)] = new("Código interno"),
                    [nameof(Product.Name)] = new("Nombre"),
                    [UnitOfMeasureCodeProperty] = new("Unidad de medida"),
                    [nameof(Product.IgvAffectation)] = new("Afectación IGV"),
                    [nameof(Product.SalePrice)] = new("Precio de venta", IsMoney: true),
                    [IsActiveProperty] = new("Activo"),
                },
                [AuditEntityType.BusinessPartner] = new Dictionary<string, Field>
                {
                    [nameof(BusinessPartner.IdentityDocumentType)] = new("Tipo de documento"),
                    [nameof(BusinessPartner.DocumentNumber)] = new("Número de documento"),
                    [nameof(BusinessPartner.CountryCode)] = new("País", Map: Countries.NameOf),
                    [nameof(BusinessPartner.Name)] = new("Nombre o razón social"),
                    [nameof(BusinessPartner.IsClient)] = new("Cliente"),
                    [nameof(BusinessPartner.IsSupplier)] = new("Proveedor"),
                    [IsActiveProperty] = new("Activo"),
                },
                [AuditEntityType.Company] = new Dictionary<string, Field>
                {
                    [nameof(Company.Ruc)] = new("RUC"),
                    [nameof(Company.Name)] = new("Razón social"),
                    [IsActiveProperty] = new("Activo"),
                },
                [AuditEntityType.User] = new Dictionary<string, Field>
                {
                    [nameof(User.Name)] = new("Nombre"),
                    [nameof(User.Email)] = new("Correo"),
                    [nameof(User.Role)] = new("Rol"),
                    [IsActiveProperty] = new("Activo"),
                },
                [AuditEntityType.Purchase] = new Dictionary<string, Field>
                {
                    [IsCancelledProperty] = new("Anulada"),
                    [nameof(Purchase.CancellationReason)] = new("Motivo de anulación"),
                },
                [AuditEntityType.UnitOfMeasure] = new Dictionary<string, Field>
                {
                    [nameof(UnitOfMeasure.Name)] = new("Nombre"),
                    [IsActiveProperty] = new("Activa"),
                },
            };

        /// <summary>Tipo y nombre legible del registro ("LIM-005 · Abrillantador…"), o null si no se audita.</summary>
        public static (AuditEntityType Type, string Label)? Describe(object entity) => entity switch
        {
            Product p => (AuditEntityType.Product, $"{p.Code} · {p.Name}"),
            BusinessPartner bp => (AuditEntityType.BusinessPartner, $"{bp.DocumentNumber} · {bp.Name}"),
            Company c => (AuditEntityType.Company, $"{c.Ruc} · {c.Name}"),
            User u => (AuditEntityType.User, $"{u.Name} · {u.Email}"),
            Purchase p => (AuditEntityType.Purchase, $"{p.Serie}-{p.Number} · {p.SupplierName}"),
            UnitOfMeasure u => (AuditEntityType.UnitOfMeasure, $"{u.Code} · {u.Name}"),
            _ => null
        };

        /// <summary>Si el campo se registra en el historial.</summary>
        public static bool IsAudited(AuditEntityType type, string property) =>
            Fields[type].ContainsKey(property);

        /// <summary>Si el campo es un estado (activo, anulada): su cambio se registra como acción.</summary>
        public static bool IsStatus(string property) =>
            property is IsActiveProperty or IsCancelledProperty;

        /// <summary>El cambio de un campo con su nombre y sus valores listos para mostrar.</summary>
        public static AuditChangeDto Change(AuditEntityType type, string property, object? from, object? to)
        {
            var field = Fields[type][property];
            return new AuditChangeDto(field.Label, Format(from, field), Format(to, field));
        }

        /// <summary>Cambio en el código de un proveedor del producto; sin valor "desde" se agregó, sin valor "a" se quitó.</summary>
        public static AuditChangeDto SupplierCodeChange(string supplierName, string? from, string? to) =>
            new($"Código de {supplierName}", from ?? "—", to ?? "—");

        private static string Format(object? value, Field field) => value switch
        {
            null => "—",
            string s when string.IsNullOrWhiteSpace(s) => "—",
            string s when field.Map is not null => field.Map(s),
            string s => s,
            bool b => b ? "Sí" : "No",
            decimal d when field.IsMoney => NumberText.Money(d),
            decimal d => NumberText.Decimal(d),
            IgvAffectation i => i.Description,
            IdentityDocumentType t => t.Description,
            UserRole r => r.Description,
            _ => value.ToString() ?? "—"
        };
    }
}
