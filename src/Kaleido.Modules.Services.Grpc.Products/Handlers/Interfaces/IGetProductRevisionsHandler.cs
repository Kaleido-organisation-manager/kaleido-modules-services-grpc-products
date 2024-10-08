namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

public interface IGetProductRevisionsHandler
{
    Task<GetProductRevisionsResponse> HandleAsync(string key, CancellationToken cancellationToken = default);
}
