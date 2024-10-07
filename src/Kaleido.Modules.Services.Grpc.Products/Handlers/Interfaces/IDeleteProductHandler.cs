namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

public interface IDeleteProductHandler
{
    Task<DeleteProductResponse> HandleAsync(string key, CancellationToken cancellationToken = default);
}