namespace ERP.Persistence.Auditing
{
    /// <summary>Un campo modificado dentro de un evento del historial. Se guarda en la columna jsonb Changes.</summary>
    internal sealed record AuditLogChange(string Field, string From, string To);
}
