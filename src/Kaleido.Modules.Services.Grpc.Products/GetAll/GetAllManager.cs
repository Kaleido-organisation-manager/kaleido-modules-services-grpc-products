
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllManager : IGetAllManager
{
    private readonly IProductsRepository _productsRepository;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetAllManager> _logger;

    public GetAllManager(
        IProductsRepository productsRepository,
        IProductPricesRepository productPricesRepository,
        IProductMapper productMapper,
        ILogger<GetAllManager> logger)
    {
        _productsRepository = productsRepository;
        _productPricesRepository = productPricesRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProducts called");
        var productEntityList = await _productsRepository.GetAllActiveAsync(cancellationToken);

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