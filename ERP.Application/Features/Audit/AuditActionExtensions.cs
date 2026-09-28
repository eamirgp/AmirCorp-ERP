namespace ERP.Application.Features.Audit
{
    public static class AuditActionExtensions
    {
        extension(AuditAction action)
        {
            public string Description => action switch
            {
                AuditAction.Created => "Creación",
                AuditAction.Updated => "Modificación",
                AuditAction.Activated => "Activación",
                AuditAction.Deactivated => "Desactivación",
                AuditAction.Cancelled => "Anulación",
                AuditAction.PasswordReset => "Cambio de contraseña",
                _ => action.ToString()
            };
        }
    }
}
