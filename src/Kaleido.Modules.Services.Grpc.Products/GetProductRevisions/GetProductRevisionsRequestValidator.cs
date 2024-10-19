using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;

public class GetProductRevisionsRequestValidator : IRequestValidator<GetProductRevisionsRequest>
{
    private readonly IProductValidator _productValidator;

    public GetProductRevisionsRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public Task<ValidationResult> ValidateAsync(GetProductRevisionsRequest request, CancellationToken cancellationToken = default)
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
