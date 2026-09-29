namespace ERP.Application.Features.Audit
{
    public static class AuditEntityTypeExtensions
    {
        extension(AuditEntityType entityType)
        {
            public string Description => entityType switch
            {
                AuditEntityType.Product => "Producto",
                AuditEntityType.BusinessPartner => "Cliente o proveedor",
                AuditEntityType.Company => "Empresa",
                AuditEntityType.User => "Usuario",
                AuditEntityType.Purchase => "Compra",
                AuditEntityType.UnitOfMeasure => "Unidad de medida",
                _ => entityType.ToString()
            };
        }
    }
}
