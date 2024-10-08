namespace Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

public interface IProductsManager
{
    Task<Product?> GetProductAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetAllProductsByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<Product> CreateProductAsync(CreateProduct product, CancellationToken cancellationToken = default);
    Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductRevision>> GetProductRevisionsAsync(string key, CancellationToken cancellationToken = default);
    Task<ProductRevision> GetProductRevisionAsync(string key, int revision, CancellationToken cancellationToken = default);
}
