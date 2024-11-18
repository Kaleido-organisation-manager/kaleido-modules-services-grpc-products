using AutoMapper;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllRevisions;

public class GetAllRevisionsManager : IGetAllRevisionsManager
{
    private readonly IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> _productLifecycleHandler;
    private readonly IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> _priceLifecycleHandler;
    private readonly IMapper _mapper;
    public GetAllRevisionsManager(
        IEntityLifecycleHandler<ProductEntity, ProductRevisionEntity> productLifecycleHandler,
        IEntityLifecycleHandler<ProductPriceEntity, ProductPriceRevisionEntity> priceLifecycleHandler,
        IMapper mapper)
    {
        _productLifecycleHandler = productLifecycleHandler;
        _priceLifecycleHandler = priceLifecycleHandler;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ManagerResponse>> GetAllRevisionsAsync(Guid key, CancellationToken cancellationToken = default)
    {
        var productRevisions = await _productLifecycleHandler.GetAllAsync(key, cancellationToken: cancellationToken);
        var priceRevisions = await _priceLifecycleHandler.FindAllAsync(x => x.ProductKey == key, cancellationToken: cancellationToken);

        // Group all changes by timestamp with a small tolerance for slight differences
        var historicTimeSlices = productRevisions
            .Select(x => x.Revision.CreatedAt)
            .Concat(priceRevisions.Select(x => x.Revision.CreatedAt))
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();


        var compositeRevisions = new List<ManagerResponse>();

        foreach (var timeSlice in historicTimeSlices)
        {
            var historicProductRevision = productRevisions.Where(x => x.Revision.CreatedAt <= timeSlice)
                .GroupBy(x => x.Key)
                .Select(x => x.OrderByDescending(y => y.Revision.Revision).First())
                .Select(x => _mapper.Map<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>(x))
                .FirstOrDefault();
            var historicPriceRevisions = priceRevisions
                .Where(x => x.Revision.CreatedAt <= timeSlice)
                .OrderByDescending(x => x.Revision.Revision)
                .GroupBy(x => x.Key)
                .Select(x => x.OrderByDescending(y => y.Revision.Revision).First())
                .Select(x => _mapper.Map<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>(x))
                .ToList();

            if (historicProductRevision != null)
            {
                compositeRevisions.Add(new ManagerResponse(historicProductRevision, historicPriceRevisions));
            }
        }

        var results = new List<ManagerResponse>();

        for (int i = 0; i < compositeRevisions.Count; i++)
        {
            var historicRevision = compositeRevisions[i];
            var historicPriceRevisions = historicRevision.ProductPrices;
            var historicProductRevision = historicRevision.Product;

            if (historicProductRevision == null || historicPriceRevisions == null)
            {
                continue;
            }

            EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>? previousProductRevision = null;
            IEnumerable<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>? previousPriceRevisions = null;

            if (i < compositeRevisions.Count - 1)
            {
                var previousHistoricRevision = compositeRevisions[i + 1];
                previousProductRevision = previousHistoricRevision.Product;
                previousPriceRevisions = previousHistoricRevision.ProductPrices;
            }

            if (previousProductRevision != null && previousProductRevision.Revision.Revision == historicProductRevision?.Revision.Revision)
            {
                historicProductRevision = _mapper.Map<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>(historicProductRevision);
                historicProductRevision.Revision.Action = RevisionAction.Unmodified;
            }

            var previousDeletedPrices = previousPriceRevisions?.Where(x => x.Revision.Action == RevisionAction.Deleted).ToList() ?? new List<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>();
            var resultingPrices = historicPriceRevisions?
                .Where(x => !previousDeletedPrices.Any(y => y.Key == x.Key))
                .Select(x =>
                {
                    if (previousPriceRevisions != null && previousPriceRevisions.Any(y => y.Key == x.Key && y.Revision.Action == x.Revision.Action))
                    {
                        x.Revision.Action = RevisionAction.Unmodified;
                    }
                    return x;
                })
                .ToList();

            if (historicProductRevision != null && resultingPrices != null)
            {
                results.Add(new ManagerResponse(historicProductRevision, resultingPrices));
            }
        }

        return results;
    }
}