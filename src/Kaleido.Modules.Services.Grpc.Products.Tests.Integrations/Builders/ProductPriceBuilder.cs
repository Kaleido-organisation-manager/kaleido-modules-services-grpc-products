using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;

public class ProductPriceBuilder
{
    private ProductPrice _productPrice = new()
    {
        CurrencyKey = Guid.NewGuid().ToString(),
        Value = 100.00f
    };

    public ProductPriceBuilder WithCurrencyKey(string currencyKey)
    {
        _productPrice.CurrencyKey = currencyKey;
        return this;
    }

    public ProductPriceBuilder WithValue(float value)
    {
        _productPrice.Value = value;
        return this;
    }

    public ProductPrice Build()
    {
        return _productPrice;
    }
}
