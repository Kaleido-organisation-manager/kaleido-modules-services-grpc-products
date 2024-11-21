using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models;

public class ProductPriceEntity : BaseEntity
{
    public Guid ProductKey { get; set; }
    public int Units { get; set; }
    public int Nanos { get; set; }
    public Guid CurrencyKey { get; set; }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj) &&
            obj is ProductPriceEntity entity &&
            ProductKey == entity.ProductKey &&
            Units == entity.Units &&
            Nanos == entity.Nanos &&
            CurrencyKey == entity.CurrencyKey;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), ProductKey, Units, Nanos, CurrencyKey);
    }
}