using ERP.Domain.Companies;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface ICompanyRepository
    {
        void Add(Company company);
        Task<bool> RucExistsAsync(string ruc, Guid? excludeId = null);
        Task<Company?> GetByIdAsync(Guid id);
    }
}
