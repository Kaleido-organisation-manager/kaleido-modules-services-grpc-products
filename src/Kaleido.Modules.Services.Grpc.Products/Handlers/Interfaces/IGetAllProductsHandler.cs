namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

public interface IGetAllProductsHandler
{
    Task<GetAllProductsResponse> HandleAsync(CancellationToken cancellationToken = default);
}