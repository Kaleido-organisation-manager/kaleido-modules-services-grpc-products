using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;

public class GetProductPriceRevisionsRequestValidator : IRequestValidator<GetProductPriceRevisionsRequest>
{
    private readonly IProductValidator _productValidator;
    private readonly IProductPriceValidator _productPriceValidator;

    public GetProductPriceRevisionsRequestValidator(
        IProductValidator productValidator,
        IProductPriceValidator productPriceValidator)
    {
        _productValidator = productValidator;
        _productPriceValidator = productPriceValidator;
    }

    public async Task<ValidationResult> ValidateAsync(GetProductPriceRevisionsRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidation = _productValidator.ValidateKeyFormat(request.Key);

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