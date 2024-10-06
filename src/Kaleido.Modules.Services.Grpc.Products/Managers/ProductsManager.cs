using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Managers;

public class ProductsManager : IProductsManager
{
    private readonly ICategoryValidator _categoryValidator;
    private readonly ILogger<ProductsManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductValidator _productValidator;
    private readonly IProductPriceValidator _productPriceValidator;
    private readonly IProductsRepository _productsRepository;

    public ProductsManager(
        ICategoryValidator categoryValidator,
        ILogger<ProductsManager> logger,
        IProductMapper productMapper,
        IProductPriceValidator productPriceValidator,
        IProductPricesRepository productPricesRepository,
        IProductValidator productValidator,
        IProductsRepository productsRepository
        )
    {
        _categoryValidator = categoryValidator;
        _logger = logger;
        _productMapper = productMapper;
        _productPriceValidator = productPriceValidator;
        _productPricesRepository = productPricesRepository;
        _productValidator = productValidator;
        _productsRepository = productsRepository;
    }

    public async Task<Product?> GetProductAsync(string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetProduct called with key: {Key}", key);
        var productEntity = await _productsRepository.GetAsync(key, cancellationToken);
        if (productEntity == null)
        {
            _logger.LogWarning("Product with key: {Key} not found", key);
            return null;
        }
        var productPrices = await _productPricesRepository.GetAllByProductIdAsync(productEntity.Key!, cancellationToken);

        return _productMapper.FromEntities(productEntity, productPrices);

        // return await _productsRepository.GetAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProducts called");
        var productEntityList = await _productsRepository.GetAllAsync(cancellationToken);

        var productList = new List<Product>();

        foreach (var productEntity in productEntityList)
        {
            var productPrices = await _productPricesRepository.GetAllByProductIdAsync(productEntity.Key!, cancellationToken);
            productList.Add(_productMapper.FromEntities(productEntity, productPrices));
        }

        return productList;
    }

    public async Task<IEnumerable<Product>> GetAllProductsByCategoryIdAsync(string categoryKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAllProductsByCategoryId called with CategoryKey: {CategoryId}", categoryKey);
        _logger.LogInformation("Validating CategoryKey: {CategoryKey}", categoryKey);
        await _categoryValidator.ValidateIdAsync(categoryKey);
        _logger.LogInformation("CategoryKey: {CategoryKey} is valid", categoryKey);

        var productEntityList = await _productsRepository.GetAllByCategoryIdAsync(categoryKey, cancellationToken);

        var productList = new List<Product>();

        foreach (var productEntity in productEntityList)
        {
            var productPrices = await _productPricesRepository.GetAllByProductIdAsync(productEntity.Key!, cancellationToken);
            productList.Add(_productMapper.FromEntities(productEntity, productPrices));
        }

        return productList;
    }

    public async Task<Product> CreateProductAsync(CreateProduct createProduct, CancellationToken cancellationToken = default)
    {
        var product = new Product()
        {
            CategoryKey = createProduct.CategoryKey,
            Description = createProduct.Description,
            Key = Guid.NewGuid().ToString(),
        };

        await _categoryValidator.ValidateIdAsync(product.CategoryKey);
        await _productValidator.ValidateCreateAsync(product);
        await _productPriceValidator.ValidateAsync(product.Prices);
        _logger.LogInformation("Saving Product with key: {Key}", product.Key);
        var productEntity = _productMapper.ToCreateEntity(product);
        var createdProductEntity = await _productsRepository.CreateAsync(productEntity, cancellationToken);

        var productPriceEntities = product.Prices.Select(price => _productMapper.ToCreatePriceEntity(createdProductEntity.Key!, price)).ToList();
        await _productPricesRepository.CreateRangeAsync(productPriceEntities, cancellationToken);

        // await _productsRepository.CreateAsync(product, cancellationToken);
        _logger.LogInformation("Product with key: {key} saved", product.Key);
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _productValidator.ValidateUpdateAsync(product);
        await _categoryValidator.ValidateIdAsync(product.CategoryKey);
        await _productPriceValidator.ValidateAsync(product.Prices);

        _logger.LogInformation("Updating Product with key: {key}", product.Key);
        var storedProduct = await _productsRepository.GetActiveAsync(product.Key!, cancellationToken);
        var newRevision = storedProduct?.Revision + 1 ?? 1;
        var productEntity = _productMapper.ToCreateEntity(product, newRevision);

        var updatedProductEntity = await _productsRepository.UpdateAsync(productEntity, cancellationToken);

        var productPriceEntities = product.Prices.Select(price => _productMapper.ToCreatePriceEntity(updatedProductEntity.Key!, price)).ToList();
        await _productPricesRepository.CreateRangeAsync(productPriceEntities, cancellationToken);

        return product;
    }
}