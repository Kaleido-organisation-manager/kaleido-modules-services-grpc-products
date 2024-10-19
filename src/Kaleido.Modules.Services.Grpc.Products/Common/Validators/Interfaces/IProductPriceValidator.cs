using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Grpc.Products;
namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface IProductPriceValidator
{
    Task<ValidationResult> ValidateAsync(IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateCurrencyKeyAsync(string currencyKey, CancellationToken cancellationToken = default);
}