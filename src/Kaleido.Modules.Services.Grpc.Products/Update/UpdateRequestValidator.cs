using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public class UpdateRequestValidator : IRequestValidator<UpdateProductRequest>
{
    private readonly IProductValidator _productValidator;

    public UpdateRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public async Task<ValidationResult> ValidateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidation = _productValidator.ValidateKeyFormat(request.Key);

        if (!keyValidation.IsValid)
        {
            validationResult.Merge(keyValidation);
        }

        var productValidation = await _productValidator.ValidateUpdateAsync(request.Product, cancellationToken);

        if (!productValidation.IsValid)
        {
            validationResult.Merge(productValidation);
        }

        return validationResult;
    }
}
