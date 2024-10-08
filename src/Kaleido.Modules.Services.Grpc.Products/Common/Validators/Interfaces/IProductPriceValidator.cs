namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface IProductPriceValidator
{
    Task ValidateAsync(IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default);
}