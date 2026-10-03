using ERP.Domain.Companies;

namespace ERP.Application.Contracts.Persistence.Commands
{
    public interface ICompanyRepository
    {
        void Add(Company company);
        /// <summary>La empresa que ya tiene ese RUC (sin contar la que se edita), o null.</summary>
        Task<Company?> FindByRucAsync(string ruc, Guid? excludeId = null);
        Task<Company?> GetByIdAsync(Guid id);

        /// <summary>Versión actual de la empresa en la base (cambia con cada modificación).</summary>
        uint VersionOf(Company company);
    }
}
