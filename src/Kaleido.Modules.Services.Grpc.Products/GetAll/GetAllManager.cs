
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllManager : IGetAllManager
{
    private readonly IProductRepository _productRepository;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetAllManager> _logger;

    public GetAllManager(
        IProductRepository productRepository,
        IProductPriceRepository productPriceRepository,
        IProductMapper productMapper,
        ILogger<GetAllManager> logger)
    {
        _productRepository = productRepository;
        _productPriceRepository = productPriceRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProducts called");
        var productEntityList = await _productRepository.GetAllActiveAsync(cancellationToken);

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