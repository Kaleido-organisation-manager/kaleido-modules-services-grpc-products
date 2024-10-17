using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Grpc.Products;


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
            categoryKeyValidationResult.RemovePathPrefix([nameof(Product)]);
            validationResult.Merge(categoryKeyValidationResult);
        }

        return validationResult;
    }
}