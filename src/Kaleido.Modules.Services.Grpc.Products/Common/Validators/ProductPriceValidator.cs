using FluentValidation;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductPriceValidator : AbstractValidator<ProductPrice>
{
    private readonly CurrencyKeyValidator _currencyKeyValidator;

    public ProductPriceValidator(CurrencyKeyValidator currencyKeyValidator)
    {
        _currencyKeyValidator = currencyKeyValidator;

        RuleFor(x => x.CurrencyKey).NotNull().NotEmpty().MustAsync(async (x, cancellationToken) => (await _currencyKeyValidator.ValidateAsync(x, cancellationToken)).IsValid);
        RuleFor(x => x.Value).GreaterThan(0);
    }
}