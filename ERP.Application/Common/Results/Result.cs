namespace ERP.Application.Common.Results
{
    public sealed class Result
    {
        public bool IsSuccess { get; }
        /// <summary>Los errores con su campo, si lo tienen (decisión 37).</summary>
        public IReadOnlyCollection<ErrorDetail> Details { get; }
        /// <summary>Solo los mensajes, para quien no necesita saber el campo.</summary>
        public IReadOnlyCollection<string> Errors { get; }
        public ErrorType? ErrorType { get; }

        private Result(bool isSuccess, IReadOnlyCollection<ErrorDetail> details, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Details = details;
            Errors = details.Select(d => d.Message).ToArray();
            ErrorType = errorType;
        }

        public static Result Success() => new(true, Array.Empty<ErrorDetail>(), null);
        public static Result Failure(IReadOnlyCollection<string> errors, ErrorType errorType) => new(false, errors.Select(e => new ErrorDetail(e)).ToArray(), errorType);
        public static Result Failure(IReadOnlyCollection<ErrorDetail> errors, ErrorType errorType) => new(false, errors, errorType);
    }
}
