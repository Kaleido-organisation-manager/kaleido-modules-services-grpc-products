namespace Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

public interface IBaseHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}