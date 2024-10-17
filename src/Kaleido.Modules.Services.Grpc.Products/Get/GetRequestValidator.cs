using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetRequestValidator : IRequestValidator<GetProductRequest>
{
    private readonly IProductValidator _productValidator;

    public GetRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public Task<ValidationResult> ValidateAsync(GetProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidation = _productValidator.ValidateKeyFormat(request.Key);

        if (!keyValidation.IsValid)
        {
            validationResult.Merge(keyValidation);
        }

        return Task.FromResult(validationResult);
    }
}