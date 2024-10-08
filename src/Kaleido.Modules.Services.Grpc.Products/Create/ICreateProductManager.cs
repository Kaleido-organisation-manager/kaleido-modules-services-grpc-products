namespace Kaleido.Modules.Services.Grpc.Products.Create;

public interface ICreateProductManager
{
    Task<CreateProductResponse> CreateAsync(CreateProduct product, CancellationToken cancellationToken = default);
}
