using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByName;

public class GetAllByNameManager : IGetAllByNameManager
{
    private readonly ILogger<GetAllByNameManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductRepository _productRepository;

    public GetAllByNameManager(
        ILogger<GetAllByNameManager> logger,
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
    public async Task<IEnumerable<Product>> GetAllByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByName called with Name: {Name}", name);

        var productEntityList = await _productRepository.GetAllByNameAsync(name, cancellationToken);

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

