using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models;

public class ProductPriceEntity : BaseEntity
{
    public required Guid ProductKey { get; set; }
    public required float Price { get; set; }
    public required Guid CurrencyKey { get; set; }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj) &&
            obj is ProductPriceEntity entity &&
            ProductKey == entity.ProductKey &&
            Price == entity.Price &&
            CurrencyKey == entity.CurrencyKey;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), ProductKey, Price, CurrencyKey);
    }
}