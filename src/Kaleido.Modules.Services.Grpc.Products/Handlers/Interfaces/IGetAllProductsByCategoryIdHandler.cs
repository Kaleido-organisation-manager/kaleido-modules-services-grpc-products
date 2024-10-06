namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

public interface IGetAllProductsByCategoryIdHandler
{
    Task<GetAllProductsResponse> HandleAsync(string categoryId, CancellationToken cancellationToken = default);
}