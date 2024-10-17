using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteManager : IDeleteManager
{
    private readonly ILogger<DeleteManager> _logger;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductRepository _productRepository;

    public DeleteManager(
        ILogger<DeleteManager> logger,
        IProductPriceRepository productPriceRepository,
        IProductRepository productRepository
        )
    {
        _logger = logger;
        _productPriceRepository = productPriceRepository;
        _productRepository = productRepository;
    }

    public async Task<ProductEntity?> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Deleting Product with key: {key}", productKey);
        var deletedEntity = await _productRepository.DeleteAsync(productKey, cancellationToken);

        if (deletedEntity == null)
        {
            return null;
        }

        await _productPriceRepository.DeleteByProductKeyAsync(productKey, cancellationToken);
        _logger.LogInformation("Product with key: {key} deleted", productKey);

        return deletedEntity;
    }
}