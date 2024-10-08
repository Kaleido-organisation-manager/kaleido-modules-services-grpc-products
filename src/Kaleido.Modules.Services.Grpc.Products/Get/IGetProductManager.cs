namespace Kaleido.Modules.Services.Grpc.Products.Get;
public interface IGetProductManager
{
    Task<Product> GetAsync(string key, CancellationToken cancellationToken = default);
}
