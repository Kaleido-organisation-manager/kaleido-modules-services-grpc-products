using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;

public class GetAllFilteredManager : IGetAllFilteredManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _priceLifecycleHandler;

    public GetAllFilteredManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> priceLifecycleHandler)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _priceLifecycleHandler = priceLifecycleHandler;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllFilteredAsync(
        string? name,
        string? categoryKey,
        CancellationToken cancellationToken)
    {
        var products = await _productLifecycleHandler.FindAllAsync(
            product =>
                (string.IsNullOrEmpty(name) || product.Name.ToLower().Contains(name.ToLower())) &&
                (string.IsNullOrEmpty(categoryKey) || product.CategoryKey == Guid.Parse(categoryKey)),
            revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
            cancellationToken: cancellationToken
        );


        var result = new List<ManagerResponse>();

        foreach (var product in products)
        {
            var prices = await _priceLifecycleHandler.FindAllAsync(
                price => price.ProductKey == product.Key,
                revision => revision.Status == RevisionStatus.Active && revision.Action != RevisionAction.Deleted,
                cancellationToken: cancellationToken
            );
            result.Add(new ManagerResponse(product, prices));
        }

        return result;
    }
}