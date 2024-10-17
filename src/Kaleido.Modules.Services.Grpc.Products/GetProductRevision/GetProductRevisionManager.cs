using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevision;

public class GetProductRevisionManager : IGetProductRevisionManager
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GetProductRevisionManager> _logger;
    private readonly IProductMapper _mapper;

    public GetProductRevisionManager(
        ILogger<GetProductRevisionManager> logger,
        IProductMapper mapper,
        IProductRepository productRepository
        )
    {
        _productRepository = productRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ProductRevision?> GetAsync(string key, int revision, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        var product = await _productRepository.GetRevisionAsync(productKey, revision, cancellationToken);
        return product == null ? null : _mapper.ToProductRevision(product);
    }
}