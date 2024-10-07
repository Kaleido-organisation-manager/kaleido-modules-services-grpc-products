namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
public interface IGetProductHandler
{
    Task<GetProductResponse> HandleAsync(string key, CancellationToken cancellationToken = default);
}