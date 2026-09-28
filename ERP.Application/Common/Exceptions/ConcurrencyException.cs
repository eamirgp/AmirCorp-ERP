namespace ERP.Application.Common.Exceptions
{
    public sealed class ConcurrencyException : Exception
    {
        public ConcurrencyException(Exception innerException)
            : base("Los datos fueron modificados por otro usuario. Vuelve a intentarlo.", innerException) { }
    }
}
