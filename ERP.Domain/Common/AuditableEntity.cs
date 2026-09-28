namespace ERP.Domain.Common
{
    public abstract class AuditableEntity : BaseEntity
    {
        // Autor de los cambios hechos por el propio sistema, sin un usuario autenticado (ej.: el seed del SuperAdmin).
        public static readonly Guid SystemUserId = Guid.Empty;

        public DateTime CreatedAt { get; private set; }
        public Guid CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        protected AuditableEntity(Guid id) : base(id) { }
    }
}
