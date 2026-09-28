namespace ERP.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; }

        protected BaseEntity(Guid id)
        {
            Id = id;
        }
    }
}
