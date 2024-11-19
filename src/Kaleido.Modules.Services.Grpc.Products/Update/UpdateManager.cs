using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public class UpdateManager : IUpdateManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _priceLifecycleHandler;
    private readonly IMapper _mapper;

    public UpdateManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> priceLifecycleHandler,
        IMapper mapper)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _priceLifecycleHandler = priceLifecycleHandler;
        _mapper = mapper;
    }

    public async Task<ManagerResponse> UpdateAsync(
        Guid key,
        ProductEntity product,
        IEnumerable<ProductPriceEntity> prices,
        CancellationToken cancellationToken = default)
    {
        var timestamp = new DateTime(DateTime.UtcNow.Ticks - (DateTime.UtcNow.Ticks % TimeSpan.TicksPerMillisecond), DateTimeKind.Utc);

        var productRevisionEntity = new ProductRevisionEntity
        {
            Key = key,
            CreatedAt = timestamp,
        };

        EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>? productResult;
        try
        {
            productResult = await _productLifecycleHandler.UpdateAsync(key, product, productRevisionEntity, cancellationToken);
        }
        catch (NotModifiedException)
        {
            productResult = await _productLifecycleHandler.GetAsync(key, cancellationToken: cancellationToken);
        }

        if (productResult == null)
        {
            return new ManagerResponse(ManagerResponseState.NotFound);
        }

        var productPrices = await _priceLifecycleHandler.FindAllAsync(price => price.ProductKey == key, cancellationToken: cancellationToken);

        // Get latest revision of product prices
        var latestProductPrices = productPrices.GroupBy(x => x.Key).Select(x => x.OrderByDescending(y => y.Revision.Revision).First());

        // Get Prices to delete
        var pricesToDelete = latestProductPrices.Where(x => !prices.Any(y => y.CurrencyKey == x.Entity.CurrencyKey) && x.Revision.Action != RevisionAction.Deleted).ToList();

        // Get Prices to create
        var pricesToCreate = prices.Where(x => !latestProductPrices.Any(y => y.Entity.CurrencyKey == x.CurrencyKey))
            .ToList();

        // Get Prices to restore
        var pricesToRestore = latestProductPrices.Where(x => prices.Any(y => y.CurrencyKey == x.Entity.CurrencyKey) && x.Revision.Action == RevisionAction.Deleted).ToList();

        var pricesToUpdate = prices.Where(x =>
            latestProductPrices.Any(y => y.Entity.CurrencyKey == x.CurrencyKey) &&
            latestProductPrices.First(y => y.Entity.CurrencyKey == x.CurrencyKey).Revision.Action != RevisionAction.Deleted &&
            latestProductPrices.First(y => y.Entity.CurrencyKey == x.CurrencyKey).Entity.Value != x.Value
        )
        .Select(x =>
        {
            var price = latestProductPrices.First(y => y.Entity.CurrencyKey == x.CurrencyKey);
            price = _mapper.Map<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>(price);
            price.Entity.Value = x.Value;
            return price;
        })
        .ToList();

        // Get the unchanged prices
        var unchangedPrices = latestProductPrices.Where(x =>
            !pricesToDelete.Any(y => y.Key == x.Key) &&
            !pricesToCreate.Any(y => y.CurrencyKey == x.Entity.CurrencyKey) &&
            !pricesToRestore.Any(y => y.Key == x.Key) &&
            !pricesToUpdate.Any(y => y.Entity.CurrencyKey == x.Entity.CurrencyKey) &&
            x.Revision.Action != RevisionAction.Deleted)
            .ToList();

        var updatedPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>();


        foreach (var price in pricesToDelete)
        {
            var priceRevisionEntity = new ProductPriceRevisionEntity
            {
                Key = price.Key,
                CreatedAt = timestamp,
            };

            var priceResult = await _priceLifecycleHandler.DeleteAsync(price.Key, priceRevisionEntity, cancellationToken);
            updatedPrices.Add(priceResult);
        }

        foreach (var price in pricesToCreate)
        {
            var priceEntity = new ProductPriceEntity
            {
                ProductKey = key,
                CurrencyKey = price.CurrencyKey,
                Value = price.Value,
            };
            var priceRevisionEntity = new ProductPriceRevisionEntity
            {
                Key = Guid.NewGuid(),
                CreatedAt = timestamp,
            };

            var priceResult = await _priceLifecycleHandler.CreateAsync(priceEntity, priceRevisionEntity, cancellationToken);
            updatedPrices.Add(priceResult);
        }

        foreach (var price in pricesToRestore)
        {
            var priceRevisionEntity = new ProductPriceRevisionEntity
            {
                Key = price.Key,
                CreatedAt = timestamp,
            };

            var priceResult = await _priceLifecycleHandler.RestoreAsync(price.Key, priceRevisionEntity, cancellationToken);
            updatedPrices.Add(priceResult);
        }

        foreach (var price in pricesToUpdate)
        {
            var priceRevisionEntity = new ProductPriceRevisionEntity
            {
                Key = price.Key,
                CreatedAt = timestamp,
            };

            var priceResult = await _priceLifecycleHandler.UpdateAsync(price.Key, price.Entity, priceRevisionEntity, cancellationToken);
            updatedPrices.Add(priceResult);
        }

        updatedPrices.AddRange(unchangedPrices.Select(x =>
        {
            x.Revision.Action = RevisionAction.Unmodified;
            return x;
        }));

        return new ManagerResponse(productResult, updatedPrices);
    }
}