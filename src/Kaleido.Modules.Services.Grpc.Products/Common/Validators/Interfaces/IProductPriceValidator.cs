using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface IProductPriceValidator
{
    Task<ValidationResult> ValidateAsync(IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateCurrencyKeyAsync(string currencyKey, CancellationToken cancellationToken = default);
}