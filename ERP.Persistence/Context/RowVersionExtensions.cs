using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Context
{
    /// <summary>
    /// La versión (xmin) de una entidad editable, la misma en todos los repositorios: el formulario la devuelve y el caso
    /// de uso la compara para no pisar el cambio de otra persona (decisión 21).
    /// </summary>
    internal static class RowVersionExtensions
    {
        /// <summary>Nombre de la propiedad sombra que cada configuración mapea a xmin.</summary>
        public const string RowVersion = "RowVersion";

        public static uint VersionOf<TEntity>(this ErpDbContext context, TEntity entity) where TEntity : class =>
            context.Entry(entity).Property<uint>(RowVersion).CurrentValue;
    }
}
