namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
public interface IGetProductHandler
{
    Task<GetProductResponse> HandleAsync(string id, CancellationToken cancellationToken = default);
}