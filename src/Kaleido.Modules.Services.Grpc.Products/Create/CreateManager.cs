using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateManager : ICreateManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _productPriceLifecycleHandler;
    private readonly ILogger<CreateManager> _logger;

    public CreateManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> productPriceLifecycleHandler,
        ILogger<CreateManager> logger)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _productPriceLifecycleHandler = productPriceLifecycleHandler;
        _logger = logger;
    }

    public async Task<ManagerResponse> CreateAsync(ProductEntity productEntity, IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow;
        var productRevision = new ProductRevisionEntity
        {
            Key = Guid.NewGuid(),
            CreatedAt = timestamp
        };

        var productPricesEntities = productPrices.Select(price => new ProductPriceEntity
        {
            CurrencyKey = Guid.Parse(price.CurrencyKey),
            ProductKey = productRevision.Key,
            Value = price.Value
        });

        var resultPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>();
        foreach (var productPrice in productPricesEntities)
        {
            var productPriceRevision = new ProductPriceRevisionEntity
            {
                Key = Guid.NewGuid(),
                CreatedAt = timestamp
            };

            var result = await _productPriceLifecycleHandler.CreateAsync(productPrice, productPriceRevision, cancellationToken: cancellationToken); ;
            resultPrices.Add(result);
        }

        var productResult = await _productLifecycleHandler.CreateAsync(productEntity, productRevision, cancellationToken: cancellationToken);


        return new ManagerResponse(productResult, resultPrices);
    }
}