
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public class GetAllByCategoryKeyManager : IGetAllByCategoryKeyManager
{

    private readonly IProductRepository _productRepository;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetAllByCategoryKeyManager> _logger;

    public GetAllByCategoryKeyManager(
        IProductRepository productRepository,
        IProductPriceRepository productPriceRepository,
        IProductMapper productMapper,
        ILogger<GetAllByCategoryKeyManager> logger)
    {
        _productRepository = productRepository;
        _productPriceRepository = productPriceRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string key, CancellationToken cancellationToken = default)
    {
        var categoryKey = Guid.Parse(key);
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryKey: {CategoryKey}", categoryKey);

        var productEntityList = await _productRepository.GetAllByCategoryIdAsync(categoryKey, cancellationToken);

        var productList = new List<Product>();

        foreach (var productEntity in productEntityList)
        {
            _logger.LogInformation("Retrieving prices for product with key: {Key}", productEntity.Key);
            var productPrices = await _productPriceRepository.GetAllActiveByProductKeyAsync(productEntity.Key!, cancellationToken);
            productList.Add(_productMapper.FromEntities(productEntity, productPrices));
        }

        return productList;
    }
}
