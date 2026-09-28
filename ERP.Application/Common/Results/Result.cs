namespace ERP.Application.Common.Results
{
    public sealed class Result
    {
        public bool IsSuccess { get; }
        public IReadOnlyCollection<string> Errors { get; }
        public ErrorType? ErrorType { get; }

        private Result(bool isSuccess, IReadOnlyCollection<string> errors, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Errors = errors;
            ErrorType = errorType;
        }

        public static Result Success() => new(true, Array.Empty<string>(), null);
        public static Result Failure(IReadOnlyCollection<string> errors, ErrorType errorType) => new(false, errors, errorType);
    }
}
