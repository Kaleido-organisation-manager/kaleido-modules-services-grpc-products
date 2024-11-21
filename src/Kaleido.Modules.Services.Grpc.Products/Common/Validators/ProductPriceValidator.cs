using FluentValidation;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductPriceValidator : AbstractValidator<ProductPrice>
{
    private readonly CurrencyKeyValidator _currencyKeyValidator;

    public ProductPriceValidator(CurrencyKeyValidator currencyKeyValidator)
    {
        _currencyKeyValidator = currencyKeyValidator;

        RuleFor(x => x.CurrencyKey).SetValidator(_currencyKeyValidator);
        RuleFor(x => x.Units).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Nanos).GreaterThanOrEqualTo(0).LessThan(100);
    }
}