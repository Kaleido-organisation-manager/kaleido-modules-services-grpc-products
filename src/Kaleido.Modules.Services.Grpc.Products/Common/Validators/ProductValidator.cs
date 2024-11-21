using FluentValidation;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator(
        CategoryKeyValidator categoryKeyValidator,
        NameValidator nameValidator,
        ProductPriceValidator productPriceValidator)
    {
        RuleFor(x => x.Name)
            .SetValidator(nameValidator);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.CategoryKey)
            .SetValidator(categoryKeyValidator)
            .WithName("Category Key");

        RuleFor(x => x.Prices)
            .NotEmpty()
            .WithMessage("At least one price is required");

        RuleForEach(x => x.Prices)
            .SetValidator(productPriceValidator);
    }
}