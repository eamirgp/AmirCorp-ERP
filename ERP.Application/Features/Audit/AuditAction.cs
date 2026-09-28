namespace ERP.Application.Features.Audit
{
    /// <summary>Qué le pasó a un registro.</summary>
    public enum AuditAction
    {
        Created = 1,
        Updated = 2,
        Activated = 3,
        Deactivated = 4,
        Cancelled = 5,
        PasswordReset = 6
    }
}
