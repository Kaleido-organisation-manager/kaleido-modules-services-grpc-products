
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public class GetAllByCategoryKeyManager : IGetAllByCategoryKeyManager
{

    private readonly IProductsRepository _productsRepository;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly ICategoryValidator _categoryValidator;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetAllByCategoryKeyManager> _logger;

    public GetAllByCategoryKeyManager(
        IProductsRepository productsRepository,
        IProductPricesRepository productPricesRepository,
        ICategoryValidator categoryValidator,
        IProductMapper productMapper,
        ILogger<GetAllByCategoryKeyManager> logger)
    {
        _productsRepository = productsRepository;
        _productPricesRepository = productPricesRepository;
        _categoryValidator = categoryValidator;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string key, CancellationToken cancellationToken = default)
    {
        var categoryKey = Guid.Parse(key);
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryKey: {CategoryKey}", categoryKey);
        _logger.LogInformation("Validating CategoryKey: {CategoryKey}", categoryKey);
        await _categoryValidator.ValidateIdAsync(categoryKey);
        _logger.LogInformation("CategoryKey: {CategoryKey} is valid", categoryKey);

        var productEntityList = await _productsRepository.GetAllByCategoryIdAsync(categoryKey, cancellationToken);

        var productList = new List<Product>();

        foreach (var productEntity in productEntityList)
        {
            _logger.LogInformation("Retrieving prices for product with key: {Key}", productEntity.Key);
            var productPrices = await _productPricesRepository.GetAllByProductIdAsync(productEntity.Key!, cancellationToken);
            productList.Add(_productMapper.FromEntities(productEntity, productPrices));
        }

        return productList;
    }
}
