
using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;

public class GetProductPriceRevisionManager : IGetProductPriceRevisionManager
{
    private readonly ILogger<GetProductPriceRevisionManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPricesRepository _productPriceRevisionRepository;

    public GetProductPriceRevisionManager(
        IProductPricesRepository productPriceRevisionRepository,
        ILogger<GetProductPriceRevisionManager> logger,
        IProductMapper productMapper
        )
    {
        _productPriceRevisionRepository = productPriceRevisionRepository;
        _logger = logger;
        _productMapper = productMapper;
    }

    public async Task<ProductPriceRevision?> GetAsync(string key, string currency, int revision, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting product price revision for key: {Key} and revision: {Revision}", key, revision);
        var productKey = Guid.Parse(key);
        var currencyKey = Guid.Parse(currency);
        var productPrice = await _productPriceRevisionRepository.GetRevisionAsync(productKey, currencyKey, revision, cancellationToken);
        return productPrice == null ? null : _productMapper.ToProductPriceRevision(productPrice);
    }
}
