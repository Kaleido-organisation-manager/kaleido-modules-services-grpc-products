using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Categories;
using static Kaleido.Grpc.Categories.GrpcCategories;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class CategoryKeyValidator : AbstractValidator<string>
{
    private readonly KeyValidator _keyValidator;
    private readonly GrpcCategoriesClient _client;
    private readonly ILogger<CategoryKeyValidator> _logger;

    public CategoryKeyValidator(KeyValidator keyValidator, GrpcCategoriesClient categoriesClient, ILogger<CategoryKeyValidator> logger)
    {
        _keyValidator = keyValidator;
        _client = categoriesClient;
        _logger = logger;

        RuleFor(x => x).NotNull().NotEmpty().Must(x => _keyValidator.Validate(x).IsValid).MustAsync(ValidateCategory).WithMessage("Category does not exist");
    }

    private async Task<bool> ValidateCategory(string category, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _client.GetCategoryAsync(new CategoryRequest { Key = category }, cancellationToken: cancellationToken);
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Category {Category} does not exist", category);
            return false;
        }
        return true;
    }
}