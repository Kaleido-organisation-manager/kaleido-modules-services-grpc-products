
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetManager : IGetManager
{
    private readonly ILogger<GetManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductRepository _productRepository;

    public GetManager(
        ILogger<GetManager> logger,
        IProductMapper productMapper,
        IProductPriceRepository productPriceRepository,
        IProductRepository productRepository
        )
    {
        _logger = logger;
        _productMapper = productMapper;
        _productPriceRepository = productPriceRepository;
        _productRepository = productRepository;
    }

    public async Task<Product?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetProduct called with key: {Key}", key);
        var productKey = Guid.Parse(key);
        var productEntity = await _productRepository.GetActiveAsync(productKey, cancellationToken);

        if (productEntity == null)
        {
            return null;
        }

        var productPrices = await _productPriceRepository.GetAllActiveByProductKeyAsync(productEntity.Key!, cancellationToken);

        return _productMapper.FromEntities(productEntity, productPrices);
    }
}