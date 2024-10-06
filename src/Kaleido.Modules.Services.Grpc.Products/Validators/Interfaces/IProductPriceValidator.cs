using Kaleido.Modules.Services.Grpc.Products.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;

public interface IProductPriceValidator
{
    Task ValidateAsync(IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default);
}