using FluentValidation;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class CurrencyKeyValidator : AbstractValidator<string>
{
    private readonly KeyValidator _keyValidator;

    public CurrencyKeyValidator(KeyValidator keyValidator)
    {
        _keyValidator = keyValidator;

        RuleFor(x => x).SetValidator(_keyValidator);
    }
}