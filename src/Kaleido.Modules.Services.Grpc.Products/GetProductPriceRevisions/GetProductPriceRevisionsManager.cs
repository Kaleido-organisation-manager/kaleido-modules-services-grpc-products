using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;

public class GetProductPriceRevisionsManager : IGetProductPriceRevisionsManager
{
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetProductPriceRevisionsManager> _logger;

    public GetProductPriceRevisionsManager(
        IProductPricesRepository productPricesRepository,
        IProductMapper productMapper,
        ILogger<GetProductPriceRevisionsManager> logger
        )
    {
        _productPricesRepository = productPricesRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductPriceRevision>> GetAllAsync(string key, string currency, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product price revisions for key: {Key} and currency: {Currency}", key, currency);
        var productKey = Guid.Parse(key);
        var currencyKey = Guid.Parse(currency);
        var productPrices = await _productPricesRepository.GetAllRevisionsAsync(productKey, currencyKey, cancellationToken);
        return productPrices.Select(p => _productMapper.ToProductPriceRevision(p));
    }
}