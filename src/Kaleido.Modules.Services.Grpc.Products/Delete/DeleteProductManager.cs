using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteProductManager : IDeleteProductManager
{
    private readonly ILogger<DeleteProductManager> _logger;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductsRepository _productsRepository;

    public DeleteProductManager(
        ILogger<DeleteProductManager> logger,
        IProductPricesRepository productPricesRepository,
        IProductsRepository productsRepository
        )
    {
        _logger = logger;
        _productPricesRepository = productPricesRepository;
        _productsRepository = productsRepository;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Deleting Product with key: {key}", productKey);
        await _productsRepository.DeleteAsync(productKey, cancellationToken);
        await _productPricesRepository.DeleteByProductKeyAsync(productKey, cancellationToken);
        _logger.LogInformation("Product with key: {key} deleted", productKey);
    }
}