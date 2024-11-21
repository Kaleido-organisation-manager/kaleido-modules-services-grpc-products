using FluentValidation;
using Grpc.Core;
using Kaleido.Grpc.Categories;
using static Kaleido.Grpc.Categories.GrpcCategories;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class CategoryKeyValidator : AbstractValidator<string>
{
    private readonly KeyValidator _keyValidator;
    private readonly GrpcCategoriesClient _categoriesClient;
    private readonly ILogger<CategoryKeyValidator> _logger;

    public CategoryKeyValidator(
        KeyValidator keyValidator,
        GrpcCategoriesClient categoriesClient,
        ILogger<CategoryKeyValidator> logger)
    {
        _keyValidator = keyValidator;
        _categoriesClient = categoriesClient;
        _logger = logger;

        RuleFor(x => x)
            .NotEmpty()
            .WithMessage("Category key must not be empty")
            .SetValidator(_keyValidator)
            .WithMessage("Invalid category key format")
            .MustAsync((value, property, context, cancellationToken) => ValidateCategory(value, cancellationToken, context))
            .WithMessage("Category does not exist");
    }

    private async Task<bool> ValidateCategory(string category, CancellationToken cancellationToken, ValidationContext<string> context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return false;
            }

            var request = new CategoryRequest { Key = category };
            var response = await _categoriesClient.GetCategoryAsync(request, cancellationToken: cancellationToken);
            return response != null;
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            _logger.LogWarning("Category {Category} does not exist", category);
            context.MessageFormatter.AppendArgument("ErrorMessage", "Category does not exist");
            return false;
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Error validating category {Category}", category);
            context.MessageFormatter.AppendArgument("ErrorMessage", ex.Status.Detail);
            return false;
        }
    }
}