using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;

public class ProductBuilder
{
    private readonly Product _product = new()
    {
        Key = Guid.NewGuid().ToString(),
        Name = "Product",
        Description = "Description",
        ImageUrl = "https://image.com/image.jpg",
        CategoryKey = Guid.NewGuid().ToString(),
        Prices = {
            new ProductPrice {
                CurrencyKey = Guid.NewGuid().ToString(),
                Value = 100
            }
        }
    };

    public ProductBuilder WithKey(string key)
    {
        _product.Key = key;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _product.Name = name;
        return this;
    }

    public ProductBuilder WithDescription(string description)
    {
        _product.Description = description;
        return this;
    }

    public ProductBuilder WithImageUrl(string imageUrl)
    {
        _product.ImageUrl = imageUrl;
        return this;
    }

    public ProductBuilder WithCategoryKey(string categoryKey)
    {
        _product.CategoryKey = categoryKey;
        return this;
    }

    public ProductBuilder WithProductPrices(IEnumerable<ProductPrice> productPrices)
    {
        _product.Prices.AddRange(productPrices);
        return this;
    }

    public Product Build() => _product;
}