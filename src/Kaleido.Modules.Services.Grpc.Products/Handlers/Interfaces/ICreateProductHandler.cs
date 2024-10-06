namespace Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

public interface ICreateProductHandler
{
    Task<CreateProductResponse> HandleAsync(CreateProduct request, CancellationToken cancellationToken = default);
}