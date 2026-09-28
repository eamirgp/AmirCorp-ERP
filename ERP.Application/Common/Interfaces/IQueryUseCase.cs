namespace ERP.Application.Common.Interfaces
{
    public interface IQueryUseCase<TResponse>
    {
        Task<TResponse> ExecuteAsync();
    }

    public interface IQueryUseCase<TRequest, TResponse>
    {
        Task<TResponse> ExecuteAsync(TRequest request);
    }
}
