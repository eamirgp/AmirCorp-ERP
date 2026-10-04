namespace ERP.Application.Common.Results
{
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        /// <summary>Los errores con su campo, si lo tienen (decisión 37).</summary>
        public IReadOnlyCollection<ErrorDetail> Details { get; }
        /// <summary>Solo los mensajes, para quien no necesita saber el campo.</summary>
        public IReadOnlyCollection<string> Errors { get; }
        public ErrorType? ErrorType { get; }

        private Result(bool isSuccess, T? value, IReadOnlyCollection<ErrorDetail> details, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Value = value;
            Details = details;
            Errors = details.Select(d => d.Message).ToArray();
            ErrorType = errorType;
        }

        public static Result<T> Success(T value) => new(true, value, Array.Empty<ErrorDetail>(), null);
        public static Result<T> Failure(IReadOnlyCollection<string> errors, ErrorType errorType) => new(false, default, errors.Select(e => new ErrorDetail(e)).ToArray(), errorType);
        public static Result<T> Failure(IReadOnlyCollection<ErrorDetail> errors, ErrorType errorType) => new(false, default, errors, errorType);

        /// <summary>Lo encontrado, o 404 con el mensaje si no existe (por ejemplo, el detalle de un registro).</summary>
        public static Result<T> FoundOr(T? value, string notFoundMessage) =>
            value is null ? Failure([notFoundMessage], Results.ErrorType.NotFound) : Success(value);
    }
}
