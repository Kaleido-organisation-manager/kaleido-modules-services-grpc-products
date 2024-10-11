
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public class GetAllByCategoryKeyManager : IGetAllByCategoryKeyManager
{

    private readonly IProductsRepository _productsRepository;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetAllByCategoryKeyManager> _logger;

    public GetAllByCategoryKeyManager(
        IProductsRepository productsRepository,
        IProductPricesRepository productPricesRepository,
        IProductMapper productMapper,
        ILogger<GetAllByCategoryKeyManager> logger)
    {
        _productsRepository = productsRepository;
        _productPricesRepository = productPricesRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string key, CancellationToken cancellationToken = default)
    {
        var categoryKey = Guid.Parse(key);
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryKey: {CategoryKey}", categoryKey);

        var productEntityList = await _productsRepository.GetAllByCategoryIdAsync(categoryKey, cancellationToken);

        var productList = new List<Product>();

        foreach (var productEntity in productEntityList)
        {
            _logger.LogInformation("Retrieving prices for product with key: {Key}", productEntity.Key);
            var productPrices = await _productPricesRepository.GetAllActiveByProductKeyAsync(productEntity.Key!, cancellationToken);
            productList.Add(_productMapper.FromEntities(productEntity, productPrices));
        }

        return productList;
    }
}
