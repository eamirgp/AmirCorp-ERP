namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
