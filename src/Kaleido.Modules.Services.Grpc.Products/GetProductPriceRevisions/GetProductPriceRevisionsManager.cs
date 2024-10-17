using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;

public class GetProductPriceRevisionsManager : IGetProductPriceRevisionsManager
{
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductMapper _productMapper;
    private readonly ILogger<GetProductPriceRevisionsManager> _logger;

    public GetProductPriceRevisionsManager(
        IProductPriceRepository productPriceRepository,
        IProductMapper productMapper,
        ILogger<GetProductPriceRevisionsManager> logger
        )
    {
        _productPriceRepository = productPriceRepository;
        _productMapper = productMapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductPriceRevision>> GetAllAsync(string key, string currency, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product price revisions for key: {Key} and currency: {Currency}", key, currency);
        var productKey = Guid.Parse(key);
        var currencyKey = Guid.Parse(currency);
        var productPrices = await _productPriceRepository.GetAllRevisionsAsync(productKey, currencyKey, cancellationToken);
        var productPriceRevisions = productPrices.Select(p => _productMapper.ToProductPriceRevision(p)).ToList();
        return productPriceRevisions;
    }
}