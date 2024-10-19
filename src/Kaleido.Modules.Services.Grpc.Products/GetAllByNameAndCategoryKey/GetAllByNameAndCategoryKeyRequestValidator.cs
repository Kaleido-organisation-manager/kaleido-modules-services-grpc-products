using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;


namespace Kaleido.Modules.Services.Grpc.Products.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyRequestValidator : IRequestValidator<GetAllProductsByNameAndCategoryKeyRequest>
{

    private readonly IProductValidator _productValidator;

    public GetAllByNameAndCategoryKeyRequestValidator(IProductValidator productValidator)
    {
        _productValidator = productValidator;
    }

    public async Task<ValidationResult> ValidateAsync(GetAllProductsByNameAndCategoryKeyRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(request.Name))
        {
            validationResult.AddRequiredError([nameof(request.Name)], "Name is required");
        }

        var categoryKeyValidationResult = await _productValidator.ValidateCategoryKeyAsync(request.CategoryKey, cancellationToken);

        if (!categoryKeyValidationResult.IsValid)
        {
            validationResult.Merge(categoryKeyValidationResult);
        }

        return validationResult;
    }
}