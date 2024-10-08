using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateProductManager : ICreateProductManager
{
    private readonly ICategoryValidator _categoryValidator;
    private readonly ILogger<CreateProductManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPriceValidator _productPriceValidator;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductValidator _productValidator;
    private readonly IProductsRepository _productsRepository;


    public CreateProductManager(
                ICategoryValidator categoryValidator,
                ILogger<CreateProductManager> logger,
                IProductMapper productMapper,
                IProductPriceValidator productPriceValidator,
                IProductPricesRepository productPricesRepository,
                IProductValidator productValidator,
                IProductsRepository productRepository
        )
    {
        _categoryValidator = categoryValidator;
        _logger = logger;
        _productMapper = productMapper;
        _productPriceValidator = productPriceValidator;
        _productPricesRepository = productPricesRepository;
        _productValidator = productValidator;
        _productsRepository = productRepository;
    }

    public async Task<CreateProductResponse> CreateAsync(CreateProduct createProduct, CancellationToken cancellationToken = default)
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

        await _categoryValidator.ValidateIdAsync(Guid.Parse(product.CategoryKey));
        await _productValidator.ValidateCreateAsync(product);
        await _productPriceValidator.ValidateAsync(product.Prices);
        _logger.LogInformation("Saving Product with key: {Key}", product.Key);
        var productEntity = _productMapper.ToCreateEntity(product);
        var createdProductEntity = await _productsRepository.CreateAsync(productEntity, cancellationToken);

        var productPriceEntities = product.Prices.Select(price => _productMapper.ToCreatePriceEntity(createdProductEntity.Key!, price)).ToList();
        await _productPricesRepository.CreateRangeAsync(productPriceEntities, cancellationToken);

        _logger.LogInformation("Product with key: {key} saved", product.Key);
        var storedProduct = _productMapper.FromEntities(createdProductEntity, productPriceEntities);
        return new CreateProductResponse
        {
            Product = storedProduct
        };
    }
}