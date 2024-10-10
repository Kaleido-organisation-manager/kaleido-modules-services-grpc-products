using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteRequestValidator : IRequestValidator<DeleteProductRequest>
{
    private readonly IProductValidator _productValidator;

    public DeleteRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public async Task<ValidationResult> ValidateAsync(DeleteProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidation = await _productValidator.ValidateKeyAsync(request.Key, cancellationToken);

        if (!keyValidation.IsValid)
        {
            validationResult.Merge(keyValidation);
        }

        return validationResult;
    }
}