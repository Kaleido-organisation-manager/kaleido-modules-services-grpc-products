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
        var timestamp = DateTime.UtcNow;

        var productRevision = new ProductRevisionEntity
        {
            Key = key,
            CreatedAt = timestamp,
        };

        EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>? productResult;
        try
        {
            productResult = await _productLifecycleHandler.UpdateAsync(key, product, productRevision, cancellationToken);
        }
        catch (NotModifiedException)
        {
            productResult = await _productLifecycleHandler.GetAsync(key, cancellationToken: cancellationToken);
        }

        if (productResult == null)
        {
            return new ManagerResponse(ManagerResponseState.NotFound);
        }

        var productPrices = await _priceLifecycleHandler.FindAllAsync(
            price => price.ProductKey == key,
            revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
            cancellationToken: cancellationToken);

        // Get Prices to delete
        var pricesToDelete = productPrices
            .Where(x => !prices.Any(y => y.CurrencyKey == x.Entity.CurrencyKey) &&
                        x.Revision.Action != RevisionAction.Deleted)
            .ToList();

        // Get Prices to create
        var pricesToCreate = prices
            .Where(x => !productPrices
                .Any(y => y.Entity.CurrencyKey == x.CurrencyKey))
            .Where(x => productPrices.FirstOrDefault(y => y.Entity.CurrencyKey == x.CurrencyKey)?.Revision.Action != RevisionAction.Deleted)
            .ToList();

        // Get Prices to restore
        var pricesToRestore = productPrices
            .Where(x => prices
                .Any(y =>
                    y.CurrencyKey == x.Entity.CurrencyKey &&
                    x.Revision.Action == RevisionAction.Deleted))
            .ToList();

        // Get Prices to update
        var pricesToUpdate = prices.Where(x =>
            productPrices.Any(y =>
                y.Entity.CurrencyKey == x.CurrencyKey &&
                (y.Entity.Units != x.Units || y.Entity.Nanos != x.Nanos) &&
                y.Revision.Action != RevisionAction.Deleted))
            .Select(x =>
            {
                var matchedActivePrice = productPrices.First(y => y.Entity.CurrencyKey == x.CurrencyKey);
                var copyOfMatched = _mapper.Map<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>(matchedActivePrice);
                x.ProductKey = key;
                copyOfMatched.Entity = x;
                return copyOfMatched;
            })
            .ToList();

        // Get the unchanged prices
        var unchangedPrices = productPrices
            .Where(x => !pricesToDelete.Any(y => y.Key == x.Key) &&
                        !pricesToCreate.Any(y => y.CurrencyKey == x.Entity.CurrencyKey) &&
                        !pricesToRestore.Any(y => y.Key == x.Key) &&
                        !pricesToUpdate.Any(y => y.Key == x.Key) &&
                        x.Revision.Action != RevisionAction.Deleted)
            .ToList();

        var updatedPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>();

        // Delete prices
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

        // Create prices
        foreach (var price in pricesToCreate)
        {
            price.ProductKey = key;
            var priceRevisionEntity = new ProductPriceRevisionEntity
            {
                Key = Guid.NewGuid(),
                CreatedAt = timestamp,
            };

            var priceResult = await _priceLifecycleHandler.CreateAsync(price, priceRevisionEntity, cancellationToken);
            updatedPrices.Add(priceResult);
        }

        // Restore prices
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

        // Update prices
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

        // Update the unchanged prices
        updatedPrices.AddRange(unchangedPrices.Select(x =>
        {
            x.Revision.Action = RevisionAction.Unmodified;
            return x;
        }));

        return new ManagerResponse(productResult, updatedPrices);
    }
}