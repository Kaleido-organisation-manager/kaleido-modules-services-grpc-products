namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

public interface IUpdateProductHandler
{
    Task<UpdateProductResponse> HandleAsync(string productKey, Product product, CancellationToken cancellationToken = default);
}