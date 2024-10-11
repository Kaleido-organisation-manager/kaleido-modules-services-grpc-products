using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteManager : IDeleteManager
{
    private readonly ILogger<DeleteManager> _logger;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductsRepository _productsRepository;

    public DeleteManager(
        ILogger<DeleteManager> logger,
        IProductPricesRepository productPricesRepository,
        IProductsRepository productsRepository
        )
    {
        _logger = logger;
        _productPricesRepository = productPricesRepository;
        _productsRepository = productsRepository;
    }

    public async Task<ProductEntity?> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Deleting Product with key: {key}", productKey);
        var deletedEntity = await _productsRepository.DeleteAsync(productKey, cancellationToken);

        if (deletedEntity == null)
        {
            return null;
        }

        await _productPricesRepository.DeleteByProductKeyAsync(productKey, cancellationToken);
        _logger.LogInformation("Product with key: {key} deleted", productKey);

        return deletedEntity;
    }
}