namespace Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

public interface IProductsManager
{
    Task<Product?> GetProductAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllProductsByCategoryIdAsync(string categoryId, CancellationToken cancellationToken = default);
    Task<Product> CreateProductAsync(CreateProduct product, CancellationToken cancellationToken = default);

    Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
}
