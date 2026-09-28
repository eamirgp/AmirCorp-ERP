namespace ERP.Api.Common
{
    public sealed record ErrorResponse(
        IReadOnlyCollection<string> Errors
        );
}
