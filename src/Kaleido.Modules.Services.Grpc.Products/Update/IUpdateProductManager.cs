namespace Kaleido.Modules.Services.Grpc.Products.Update;

public interface IUpdateProductManager
{
    Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default);
}