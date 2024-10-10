
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public class UpdateManager : IUpdateManager
{
    private readonly ILogger<UpdateManager> _logger;
    private readonly IProductMapper _productMapper;
    private readonly IProductPricesRepository _productPricesRepository;
    private readonly IProductsRepository _productsRepository;

    public UpdateManager(
        ILogger<UpdateManager> logger,
        IProductMapper productMapper,
        IProductPricesRepository productPricesRepository,
        IProductsRepository productsRepository
        )
    {
        _logger = logger;
        _productMapper = productMapper;
        _productPricesRepository = productPricesRepository;
        _productsRepository = productsRepository;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating Product with key: {key}", product.Key);
        var productKey = Guid.Parse(product.Key);
        var storedProduct = await _productsRepository.GetAsync(productKey, cancellationToken);

        if (storedProduct == null)
        {
            throw new ArgumentException($"Product with key: {product.Key} not found");
        }

        var newRevision = storedProduct!.Revision + 1;
        var productEntity = _productMapper.ToCreateEntity(product, newRevision);

        var updatedProductEntity = storedProduct;
        if (!storedProduct.Equals(productEntity))
        {
            _logger.LogInformation("Product with key: {key} has changed, updating", product.Key);
            updatedProductEntity = await _productsRepository.UpdateAsync(productEntity, cancellationToken);
        }

        var storedProductPrices = await _productPricesRepository.GetAllByProductKeyAsync(productKey, cancellationToken);

        var productPriceEntities = new List<ProductPriceEntity>();

        foreach (var storedProductPrice in storedProductPrices)
        {
            var incomingProductPrice = product.Prices.FirstOrDefault(price => Guid.Parse(price.CurrencyKey).Equals(storedProductPrice.CurrencyKey));
            if (incomingProductPrice == null)
            {
                _logger.LogInformation("ProductPrice with key: {key} not found, archiving", storedProductPrice.Key);
                await _productPricesRepository.UpdateStatusAsync(storedProductPrice.Key!, EntityStatus.Archived, cancellationToken);
            }
            else if (!storedProductPrice.Equals(_productMapper.ToCreatePriceEntity(storedProduct.Key!, incomingProductPrice!)))
            {
                _logger.LogInformation("ProductPrice with key: {key} has changed, updating", storedProductPrice.Key);
                var productPrice = _productMapper.ToCreatePriceEntity(storedProduct.Key!, incomingProductPrice, storedProductPrice.Revision + 1);
                var updatedProductPrice = await _productPricesRepository.UpdateAsync(productPrice, cancellationToken);
                productPriceEntities.Add(updatedProductPrice);
            }
            else
            {
                _logger.LogInformation("ProductPrice with key: {key} has not changed", storedProductPrice.Key);
                productPriceEntities.Add(storedProductPrice);
            }
        }

        // check for any new product price entities
        var newProductPrices = product.Prices.Where(price => storedProductPrices.All(storedPrice => storedPrice.CurrencyKey != Guid.Parse(price.CurrencyKey)));
        foreach (var newProductPrice in newProductPrices)
        {
            _logger.LogInformation("New ProductPrice found for currency {currencyKey}, creating", newProductPrice.CurrencyKey);
            var productPrice = _productMapper.ToCreatePriceEntity(updatedProductEntity.Key, newProductPrice);
            var createdProductPrice = await _productPricesRepository.CreateAsync(productPrice, cancellationToken);
            productPriceEntities.Add(createdProductPrice);
        }

        return _productMapper.FromEntities(updatedProductEntity, productPriceEntities);
    }
}