using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductValidator : IProductValidator
{
    private readonly IProductsRepository _productsRepository;

    public ProductValidator(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<ValidationResult> ValidateCreateAsync(CreateProduct createProduct, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var product = new Product
        {
            Name = createProduct.Name,
            CategoryKey = createProduct.CategoryKey,
            Description = createProduct.Description,
            ImageUrl = createProduct.ImageUrl,
            Prices = { createProduct.Prices },
        };

        var commonValidationResult = await ValidateCommonRules(product, cancellationToken);
        if (!commonValidationResult.IsValid)
        {
            validationResult.Merge(commonValidationResult);
        }

        return validationResult;
    }

    public async Task<ValidationResult> ValidateUpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var keyValidationResult = ValidateKeyFormat(product.Key);
        if (!keyValidationResult.IsValid)
        {
            validationResult.Merge(keyValidationResult);
        }

        var commonValidationResult = await ValidateCommonRules(product, cancellationToken);
        if (!commonValidationResult.IsValid)
        {
            validationResult.Merge(commonValidationResult);
        }

        return validationResult;
    }

    public async Task<ValidationResult> ValidateKeyAsync(string productKey, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        var commonKeyValidationResult = ValidateCommonRulesForProductKey(productKey, out var guid);
        if (!commonKeyValidationResult.IsValid)
        {
            validationResult.Merge(commonKeyValidationResult);
        }

        var existingProduct = await _productsRepository.GetActiveAsync(guid, cancellationToken);
        if (existingProduct == null)
        {
            validationResult.AddNotFoundError([nameof(Product), nameof(productKey)], "Product not found");
        }

        return validationResult;
    }

    public async Task<ValidationResult> ValidateCategoryKeyAsync(string categoryKey, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(categoryKey))
        {
            validationResult.AddRequiredError([nameof(Product), nameof(categoryKey)], "Product CategoryId is required");
        }

        if (!Guid.TryParse(categoryKey, out var guid))
        {
            validationResult.AddInvalidFormatError([nameof(Product), nameof(categoryKey)], "Product CategoryId is not a valid GUID");
        }

        // TODO: Check if category exists using the category service

        // This is put here to avoid the warning about async
        await Task.CompletedTask;

        return validationResult;
    }

    public ValidationResult ValidateKeyFormat(string productKey)
    {
        return ValidateCommonRulesForProductKey(productKey, out var _);
    }

    private ValidationResult ValidateCommonRulesForProductKey(string productKey, out Guid guid)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(productKey))
        {
            validationResult.AddRequiredError([nameof(Product), nameof(productKey)], "Product Key is required");
        }

        if (!Guid.TryParse(productKey, out guid))
        {
            validationResult.AddInvalidFormatError([nameof(Product), nameof(productKey)], "Product Key is not a valid GUID");
        }

        return validationResult;
    }

    private async Task<ValidationResult> ValidateCommonRules(Product product, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            validationResult.AddRequiredError([nameof(Product), nameof(product.Name)], "Product Name is required");
        }

        // Product name can be at most 100 characters
        if (!string.IsNullOrWhiteSpace(product.Name) && product.Name.Length > 100)
        {
            validationResult.AddInvalidFormatError([nameof(Product), nameof(product.Name)], "Product Name must be at most 100 characters");
        }

        var categoryValidationResult = await ValidateCategoryKeyAsync(product.CategoryKey, cancellationToken);
        if (!categoryValidationResult.IsValid)
        {
            validationResult.Merge(categoryValidationResult);
        }

        // description is not required but if it is provided, it can be at most 500 characters
        if (!string.IsNullOrWhiteSpace(product.Description) && product.Description.Length > 500)
        {
            validationResult.AddInvalidFormatError([nameof(Product), nameof(product.Description)], "Product Description must be at most 500 characters");
        }

        return validationResult;
    }
}