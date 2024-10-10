using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;

public class GetProductPriceRevisionRequestValidator : IRequestValidator<GetProductPriceRevisionRequest>
{
    private readonly IProductValidator _productValidator;
    private readonly IProductPriceValidator _productPriceValidator;
    public GetProductPriceRevisionRequestValidator(
        IProductValidator productValidator,
        IProductPriceValidator productPriceValidator)
    {
        _productValidator = productValidator;
        _productPriceValidator = productPriceValidator;
    }

    public async Task<ValidationResult> ValidateAsync(GetProductPriceRevisionRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidation = await _productValidator.ValidateKeyForRevisionAsync(request.Key, cancellationToken);

        if (!keyValidation.IsValid)
        {
            validationResult.Merge(keyValidation);
        }

        var currentPriceValidation = await _productPriceValidator.ValidateCurrencyKeyAsync(request.CurrencyKey, cancellationToken);

        if (!currentPriceValidation.IsValid)
        {
            validationResult.Merge(currentPriceValidation);
        }

        return validationResult;
    }
}