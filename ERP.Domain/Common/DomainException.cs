namespace ERP.Domain.Common
{
    public sealed class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }

        /// <summary>
        /// Lanza el error si la regla lo devolvió. Las reglas del dominio son funciones <c>…Error</c> que devuelven el
        /// mensaje o null (decisión 20): la entidad las usa con esto, y la API y los casos de uso las llaman antes.
        /// </summary>
        public static void ThrowIf(string? error)
        {
            if (error is not null)
                throw new DomainException(error);
        }
    }
}
