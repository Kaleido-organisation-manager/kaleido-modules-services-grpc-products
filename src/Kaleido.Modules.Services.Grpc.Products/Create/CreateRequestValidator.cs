using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateRequestValidator : IRequestValidator<CreateProductRequest>
{
    private readonly IProductValidator _productValidator;
    private readonly IProductPriceValidator _productPriceValidator;

    public CreateRequestValidator(
        IProductValidator productValidator,
        IProductPriceValidator productPriceValidator
        )
    {
        _productValidator = productValidator;
        _productPriceValidator = productPriceValidator;
    }

    public async Task<ValidationResult> ValidateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var createProductValidation = await _productValidator.ValidateCreateAsync(request.Product, cancellationToken);

        if (!createProductValidation.IsValid)
        {
            validationResult.Merge(createProductValidation);
        }

        var productPricesValidation = await _productPriceValidator.ValidateAsync(request.Product.Prices, cancellationToken);

        if (!productPricesValidation.IsValid)
        {
            validationResult.Merge(productPricesValidation);
        }

        return validationResult;
    }
}
