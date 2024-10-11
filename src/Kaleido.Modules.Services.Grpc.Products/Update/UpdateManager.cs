
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

    public async Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating Product with key: {key}", product.Key);
        var productKey = Guid.Parse(product.Key);
        var storedProduct = await _productsRepository.GetActiveAsync(productKey, cancellationToken);

        if (storedProduct == null)
        {
            return null;
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

        var latestStoredProductPrices = storedProductPrices.GroupBy(x => x.CurrencyKey)
            .Select(g => g.OrderByDescending(x => x.Revision).First());

        foreach (var storedProductPrice in latestStoredProductPrices)
        {
            var incomingProductPrice = product.Prices.FirstOrDefault(price => Guid.Parse(price.CurrencyKey).Equals(storedProductPrice.CurrencyKey));
            if (incomingProductPrice == null)
            {
                _logger.LogInformation("ProductPrice with key: {key} not found, archiving", storedProductPrice.Key);
                await _productPricesRepository.UpdateStatusAsync(storedProductPrice.Key!, EntityStatus.Archived, cancellationToken);
            }
            else if (!storedProductPrice.Equals(_productMapper.ToCreatePriceEntity(storedProduct.Key!, incomingProductPrice)))
            {
                _logger.LogInformation("ProductPrice with key: {key} has changed, updating", storedProductPrice.Key);
                var newProductPrice = _productMapper.ToCreatePriceEntity(storedProduct.Key!, incomingProductPrice, storedProductPrice.Key, storedProductPrice.Revision + 1);
                var updatedProductPrice = await _productPricesRepository.UpdateAsync(newProductPrice, cancellationToken);
                productPriceEntities.Add(updatedProductPrice);
            }
            else
            {
                productPriceEntities.Add(storedProductPrice);
            }
        }

        var newProductPrices = product.Prices.Where(price => latestStoredProductPrices.All(storedPrice => storedPrice.CurrencyKey != Guid.Parse(price.CurrencyKey)));

        foreach (var newProductPrice in newProductPrices)
        {
            _logger.LogInformation("New ProductPrice found for currency {currencyKey}, creating", newProductPrice.CurrencyKey);
            var productPrice = _productMapper.ToCreatePriceEntity(updatedProductEntity.Key, newProductPrice, revision: 1);
            var createdProductPrice = await _productPricesRepository.CreateAsync(productPrice, cancellationToken);
            productPriceEntities.Add(createdProductPrice);
        }

        var updatedProduct = _productMapper.FromEntities(updatedProductEntity, productPriceEntities);
        return updatedProduct;
    }
}