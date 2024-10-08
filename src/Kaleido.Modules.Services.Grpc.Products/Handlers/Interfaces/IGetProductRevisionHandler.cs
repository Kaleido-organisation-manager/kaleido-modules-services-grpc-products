namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
public interface IGetProductRevisionHandler
{
    Task<GetProductRevisionResponse> HandleAsync(string key, int revision, CancellationToken cancellationToken = default);
}
