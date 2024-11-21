using FluentValidation;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class NameValidator : AbstractValidator<string>
{
    public NameValidator()
    {
        RuleFor(x => x).NotNull().NotEmpty().MaximumLength(100);
    }
}