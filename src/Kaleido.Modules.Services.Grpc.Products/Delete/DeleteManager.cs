using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteManager : IDeleteManager
{

    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _productPriceLifecycleHandler;

    public DeleteManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> productPriceLifecycleHandler)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _productPriceLifecycleHandler = productPriceLifecycleHandler;
    }

    public async Task<ManagerResponse> DeleteAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var requestedProduct = await _productLifecycleHandler.GetAsync(key, cancellationToken: cancellationToken);

        if (requestedProduct == null)
        {
            return new ManagerResponse(ManagerResponseState.NotFound);
        }

        var productPrices = await _productPriceLifecycleHandler.FindAllAsync(
            link => link.ProductKey == key,
            cancellationToken: cancellationToken);

        productPrices = productPrices.GroupBy(l => l.Key)
            .Select(l => l.OrderByDescending(x => x.Revision.Revision).First())
            .Where(l => l.Revision.Action != RevisionAction.Deleted);

        var timestamp = DateTime.UtcNow;

        var resultPrices = new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>();
        foreach (var productPrice in productPrices)
        {
            var productPriceRevision = new ProductPriceRevisionEntity
            {
                Key = productPrice.Key,
                CreatedAt = timestamp
            };

            var result = await _productPriceLifecycleHandler.DeleteAsync(productPrice.Key, productPriceRevision, cancellationToken: cancellationToken);
            if (result != null)
            {
                resultPrices.Add(result);
            }
        }

        var productRevision = new ProductRevisionEntity
        {
            Key = key,
            CreatedAt = timestamp
        };
        var productResult = await _productLifecycleHandler.DeleteAsync(key, productRevision, cancellationToken: cancellationToken);

        return new ManagerResponse(productResult, resultPrices);
    }
}
