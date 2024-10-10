using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;

public class GetProductRevisionsManager : IGetProductRevisionsManager
{
    private readonly IProductsRepository _productRepository;
    private readonly ILogger<GetProductRevisionsManager> _logger;
    private readonly IProductMapper _mapper;

    public GetProductRevisionsManager(
        ILogger<GetProductRevisionsManager> logger,
        IProductMapper mapper,
        IProductsRepository productRepository
        )
    {
        _productRepository = productRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductRevision>> GetAllAsync(string key, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Getting all product revisions for key: {Key}", key);
        var products = await _productRepository.GetAllRevisionsAsync(productKey, cancellationToken);
        return products.Select(_mapper.ToProductRevision).ToList();
    }
}
