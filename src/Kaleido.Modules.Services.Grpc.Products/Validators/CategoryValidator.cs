using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Validators;

public class CategoryValidator : ICategoryValidator
{

    private readonly ILogger<CategoryValidator> _logger;

    public CategoryValidator(ILogger<CategoryValidator> logger)
    {
        _logger = logger;
    }

    public Task ValidateIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating CategoryId: {CategoryId}", id);

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ValidationException("Category id can not be null or empty.");
        }

        // TODO: Add validation logic that checks if the category exists against the category gRPC service

        return Task.CompletedTask;
    }
}
