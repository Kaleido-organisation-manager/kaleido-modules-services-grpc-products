
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllProductManager : IGetAllProductManager
{
    private readonly IProductsRepository _productsRepository;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetAllProductManager> _logger;

    public GetAllProductManager(
        IProductsRepository productsRepository,
        IProductPricesRepository productPricesRepository,
        IProductMapper productMapper,
        ILogger<GetAllProductManager> logger)
    {
        _productsRepository = productsRepository;
        _productPricesRepository = productPricesRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProducts called");
        var productEntityList = await _productsRepository.GetAllAsync(cancellationToken);

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