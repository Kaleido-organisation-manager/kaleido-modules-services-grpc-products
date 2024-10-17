using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateManager : ICreateManager
{
    private readonly ILogger<CreateManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPriceRepository _productPriceRepository;
    private readonly IProductRepository _productRepository;


    public CreateManager(
                ILogger<CreateManager> logger,
                IProductMapper productMapper,
                IProductPriceRepository productPriceRepository,
                IProductRepository productRepository
        )
    {
        _logger = logger;
        _productMapper = productMapper;
        _productPriceRepository = productPriceRepository;
        _productRepository = productRepository;
    }

    public async Task<Product> CreateAsync(CreateProduct createProduct, CancellationToken cancellationToken = default)
    {
        var priceList = createProduct.Prices.Select(price => new ProductPrice
        {
            CurrencyKey = price.CurrencyKey,
            Value = price.Value
        }).ToList();

        var product = new Product()
        {
            CategoryKey = createProduct.CategoryKey,
            Description = createProduct.Description,
            Key = Guid.NewGuid().ToString(),
            Name = createProduct.Name,
            ImageUrl = createProduct.ImageUrl,
            Prices = { priceList }
        };

        _logger.LogInformation("Saving Product with key: {Key}", product.Key);
        var productEntity = _productMapper.ToCreateEntity(product);
        var createdProductEntity = await _productRepository.CreateAsync(productEntity, cancellationToken);



        var productPriceEntities = product.Prices.Select(price => _productMapper.ToCreatePriceEntity(createdProductEntity.Key!, price)).ToList();
        if (productPriceEntities.Any())
        {
            await _productPriceRepository.CreateRangeAsync(productPriceEntities, cancellationToken);
        }

        _logger.LogInformation("Product with key: {key} saved", product.Key);
        var storedProduct = _productMapper.FromEntities(createdProductEntity, productPriceEntities);
        return storedProduct;
    }
}