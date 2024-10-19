using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;

public class GetAllByCategoryKeyRequestValidator : IRequestValidator<GetAllProductsByCategoryKeyRequest>
{
    private readonly IProductValidator _productValidator;

    public GetAllByCategoryKeyRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public async Task<ValidationResult> ValidateAsync(GetAllProductsByCategoryKeyRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidation = await _productValidator.ValidateCategoryKeyAsync(request.CategoryKey, cancellationToken);

        if (!keyValidation.IsValid)
        {
            validationResult.Merge(keyValidation);
        }

        return validationResult;
    }
}