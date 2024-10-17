using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyManager : IGetAllByNameAndCategoryKeyManager
{
    private readonly ILogger<GetAllByNameAndCategoryKeyManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductRepository _productRepository;

    public GetAllByNameAndCategoryKeyManager(
        ILogger<GetAllByNameAndCategoryKeyManager> logger,
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
    public async Task<IEnumerable<Product>> GetAllByNameAndCategoryKeyAsync(string name, string categoryKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByNameAndCategoryKey called with Name: {Name} and CategoryKey: {CategoryKey}", name, categoryKey);

        var categoryKeyGuid = Guid.Parse(categoryKey);
        var productEntityList = await _productRepository.GetAllByNameAndCategoryKeyAsync(name, categoryKeyGuid, cancellationToken);

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
