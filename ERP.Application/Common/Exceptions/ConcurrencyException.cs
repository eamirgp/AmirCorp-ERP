namespace ERP.Application.Common.Exceptions
{
    /// <summary>Otra persona cambió los mismos datos al mismo tiempo. La API responde 409 con el mensaje.</summary>
    public sealed class ConcurrencyException : Exception
    {
        public ConcurrencyException(Exception innerException)
            : base("Los datos fueron modificados por otro usuario. Vuelve a intentarlo.", innerException) { }

        public ConcurrencyException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
