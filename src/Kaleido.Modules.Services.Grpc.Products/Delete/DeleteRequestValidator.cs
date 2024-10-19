using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteRequestValidator : IRequestValidator<DeleteProductRequest>
{
    private readonly IProductValidator _productValidator;

    public DeleteRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public Task<ValidationResult> ValidateAsync(DeleteProductRequest request, CancellationToken cancellationToken = default)
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