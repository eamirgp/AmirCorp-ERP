using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Results;

namespace ERP.Application.Features.Users.EnsureSuperAdmin
{
    /// <summary>
    /// Crea el SuperAdmin si todavía no existe. El valor del resultado indica si fue creado en esta ejecución.
    /// </summary>
    public interface IEnsureSuperAdminUseCase : IUseCase<EnsureSuperAdminDto, Result<bool>> { }
}
