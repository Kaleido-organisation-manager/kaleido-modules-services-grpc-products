using FluentValidation;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    private readonly CategoryKeyValidator _categoryKeyValidator;
    private readonly NameValidator _nameValidator;
    private readonly ProductPriceValidator _productPriceValidator;

    public ProductValidator(CategoryKeyValidator categoryKeyValidator, NameValidator nameValidator, ProductPriceValidator productPriceValidator)
    {
        _categoryKeyValidator = categoryKeyValidator;
        _nameValidator = nameValidator;
        _productPriceValidator = productPriceValidator;

        RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(100).MustAsync(async (x, cancellationToken) => (await _nameValidator.ValidateAsync(x, cancellationToken)).IsValid);
        RuleFor(x => x.CategoryKey).NotNull().NotEmpty().MustAsync(async (x, cancellationToken) => (await _categoryKeyValidator.ValidateAsync(x, cancellationToken)).IsValid);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Prices).NotNull().NotEmpty().ChildRules(x => x.RuleForEach(x => x).MustAsync(async (x, cancellationToken) => (await _productPriceValidator.ValidateAsync(x, cancellationToken)).IsValid));
    }
}