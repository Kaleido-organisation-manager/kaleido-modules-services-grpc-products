using Grpc.Core;

namespace Kaleido.Modules.Services.Grpc.Products.Models;

public class ProductPriceEntity : BaseEntity
{
    public required string ProductKey { get; set; }
    public required float Price { get; set; }
    public required string CurrencyKey { get; set; }
}