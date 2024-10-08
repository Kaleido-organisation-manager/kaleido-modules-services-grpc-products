using Kaleido.Modules.Services.Grpc.Products.Constants;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Models;
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
        var productKey = Guid.Parse(key);
        var productEntity = await _productsRepository.GetAsync(productKey, cancellationToken);
        if (productEntity == null)
        {
            _logger.LogWarning("Product with key: {Key} not found", key);
            return null;
        }
        var productPrices = await _productPricesRepository.GetAllByProductIdAsync(productEntity.Key!, cancellationToken);

        return _productMapper.FromEntities(productEntity, productPrices);
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

    public async Task<IEnumerable<Product>> GetAllProductsByCategoryIdAsync(Guid categoryKey, CancellationToken cancellationToken = default)
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
        return storedProduct;
    }

    public async Task<Product> UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _productValidator.ValidateUpdateAsync(product);
        await _categoryValidator.ValidateIdAsync(Guid.Parse(product.CategoryKey));
        await _productPriceValidator.ValidateAsync(product.Prices);

        _logger.LogInformation("Updating Product with key: {key}", product.Key);
        var productKey = Guid.Parse(product.Key);
        var storedProduct = await _productsRepository.GetAsync(productKey, cancellationToken);
        var newRevision = storedProduct.Revision + 1;
        var productEntity = _productMapper.ToCreateEntity(product, newRevision);

        ProductEntity updatedProductEntity = storedProduct;
        if (!storedProduct!.Equals(productEntity))
        {
            updatedProductEntity = await _productsRepository.UpdateAsync(productEntity, cancellationToken);
        }

        var storedProductPrices = await _productPricesRepository.GetAllByProductIdAsync(productKey, cancellationToken);

        var productPriceEntities = new List<ProductPriceEntity>();

        foreach (var storedProductPrice in storedProductPrices)
        {
            var incomingProductPrice = product.Prices.FirstOrDefault(price => Guid.Parse(price.CurrencyKey).Equals(storedProductPrice.CurrencyKey));
            if (incomingProductPrice == null)
            {
                await _productPricesRepository.UpdateStatusAsync(storedProductPrice.Key!, EntityStatus.Archived, cancellationToken);
            }
            else if (!storedProductPrice.Equals(_productMapper.ToCreatePriceEntity(storedProduct.Key!, incomingProductPrice!)))
            {
                var productPrice = _productMapper.ToCreatePriceEntity(storedProduct.Key!, incomingProductPrice, storedProductPrice.Revision + 1);
                var updatedProductPrice = await _productPricesRepository.UpdateAsync(productPrice, cancellationToken);
                productPriceEntities.Add(updatedProductPrice);
            }
            else
            {
                productPriceEntities.Add(storedProductPrice);
            }
        }

        // check for any new product price entities
        var newProductPrices = product.Prices.Where(price => storedProductPrices.All(storedPrice => storedPrice.CurrencyKey != Guid.Parse(price.CurrencyKey)));
        foreach (var newProductPrice in newProductPrices)
        {
            var productPrice = _productMapper.ToCreatePriceEntity(updatedProductEntity.Key!, newProductPrice);
            var createdProductPrice = await _productPricesRepository.CreateAsync(productPrice, cancellationToken);
            productPriceEntities.Add(createdProductPrice);
        }

        return _productMapper.FromEntities(updatedProductEntity, productPriceEntities);
    }

    public async Task DeleteProductAsync(string key, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Deleting Product with key: {key}", productKey);
        await _productsRepository.DeleteAsync(productKey, cancellationToken);
        await _productPricesRepository.DeleteByProductKeyAsync(productKey, cancellationToken);
        _logger.LogInformation("Product with key: {key} deleted", productKey);
    }

    public async Task<IEnumerable<ProductRevision>> GetProductRevisionsAsync(string key, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Getting Product Revisions for key: {key}", productKey);
        var productRevisions = await _productsRepository.GetRevisionsAsync(productKey, cancellationToken);
        return productRevisions.Select(revision => _productMapper.ToProductRevision(revision));
    }

    public async Task<ProductRevision> GetProductRevisionAsync(string key, int revision, CancellationToken cancellationToken = default)
    {
        var productKey = Guid.Parse(key);
        _logger.LogInformation("Getting Product Revision for key: {key} and revision: {revision}", productKey, revision);
        var productRevision = await _productsRepository.GetRevisionAsync(productKey, revision, cancellationToken);
        return _productMapper.ToProductRevision(productRevision);
    }
}