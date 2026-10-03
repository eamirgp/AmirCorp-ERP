namespace ERP.Application.Common.Results
{
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public IReadOnlyCollection<string> Errors { get; }
        public ErrorType? ErrorType { get; }

        private Result(bool isSuccess, T? value, IReadOnlyCollection<string> errors, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Value = value;
            Errors = errors;
            ErrorType = errorType;
        }

        public static Result<T> Success(T value) => new(true, value, Array.Empty<string>(), null);
        public static Result<T> Failure(IReadOnlyCollection<string> errors, ErrorType errorType) => new(false, default, errors, errorType);

        /// <summary>Lo encontrado, o 404 con el mensaje si no existe (por ejemplo, el detalle de un registro).</summary>
        public static Result<T> FoundOr(T? value, string notFoundMessage) =>
            value is null ? Failure([notFoundMessage], Results.ErrorType.NotFound) : Success(value);
    }
}
