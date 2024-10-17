using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductPriceValidator : IProductPriceValidator
{

    public async Task<ValidationResult> ValidateAsync(IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        foreach (var productPrice in productPrices)
        {
            var priceIndex = productPrices.ToList().IndexOf(productPrice);
            if (productPrice.Value < 0)
            {
                validationResult.AddError([priceIndex.ToString(), nameof(productPrice.Value)], ValidationErrorType.InvalidFormat, "Price must be greater than or equal to 0");
            }

            var currencyValidationResult = await ValidateCurrencyKeyAsync(productPrice.CurrencyKey, cancellationToken);
            if (!currencyValidationResult.IsValid)
            {
                validationResult.Merge(currencyValidationResult.PrependPath([priceIndex.ToString()]));
            }
        }

        return validationResult;
    }

    public async Task<ValidationResult> ValidateCurrencyKeyAsync(string currencyKey, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(currencyKey))
        {
            validationResult.AddError([nameof(currencyKey)], ValidationErrorType.Required, "Currency Key is required");
        }

        if (!Guid.TryParse(currencyKey, out var currencyGuid))
        {
            validationResult.AddError([nameof(currencyKey)], ValidationErrorType.InvalidFormat, "Currency Key is not a valid GUID");
        }

        // TODO: Check if currency exists using the currency service

        return await Task.FromResult(validationResult);
    }
}