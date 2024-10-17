using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
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
            keyValidation.RemovePathPrefix([nameof(Product)]);
            validationResult.Merge(keyValidation);
        }

        return validationResult;
    }
}