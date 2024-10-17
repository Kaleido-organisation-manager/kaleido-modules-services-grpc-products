using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;

public class CreateProductBuilder
{
    private readonly CreateProduct _product = new()
    {
        CategoryKey = Guid.NewGuid().ToString(),
        Name = "Product",
        Description = "Description",
        ImageUrl = "https://image.com/image.jpg",
        Prices = {
            new ProductPrice {
                CurrencyKey = Guid.NewGuid().ToString(),
                Value = 100
            }
        }
    };

    public CreateProductBuilder WithName(string name)
    {
        _product.Name = name;
        return this;
    }

    public CreateProductBuilder WithDescription(string description)
    {
        _product.Description = description;
        return this;
    }

    public CreateProductBuilder WithImageUrl(string imageUrl)
    {
        _product.ImageUrl = imageUrl;
        return this;
    }

    public CreateProductBuilder WithCategoryKey(string categoryKey)
    {
        _product.CategoryKey = categoryKey;
        return this;
    }

    public CreateProductBuilder WithProductPrices(IEnumerable<ProductPrice> productPrices)
    {
        _product.Prices.AddRange(productPrices);
        return this;
    }

    public CreateProduct Build() => _product;



}