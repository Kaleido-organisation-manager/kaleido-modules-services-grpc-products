
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetProductManager : IGetProductManager
{
    private readonly ILogger<GetProductManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductsRepository _productsRepository;

    public GetProductManager(
        ILogger<GetProductManager> logger,
        IProductMapper productMapper,
        IProductPricesRepository productPricesRepository,
        IProductsRepository productsRepository
        )
    {
        _logger = logger;
        _productMapper = productMapper;
        _productPricesRepository = productPricesRepository;
        _productsRepository = productsRepository;
    }

    public async Task<Product> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetProduct called with key: {Key}", key);
        var productKey = Guid.Parse(key);
        var productEntity = await _productsRepository.GetAsync(productKey, cancellationToken);
        var productPrices = await _productPricesRepository.GetAllByProductIdAsync(productEntity.Key!, cancellationToken);

        return _productMapper.FromEntities(productEntity, productPrices);
    }
}