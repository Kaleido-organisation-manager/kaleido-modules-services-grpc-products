using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class CategoryValidator : ICategoryValidator
{

    private readonly ILogger<CategoryValidator> _logger;

    public CategoryValidator(ILogger<CategoryValidator> logger)
    {
        _logger = logger;
    }

    public Task ValidateIdAsync(Guid key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating CategoryKey: {CategoryId}", key);

        if (Guid.Empty == key)
        {
            throw new ValidationException("Category key can not be null or empty.");
        }

        // TODO: Add validation logic that checks if the category exists against the category gRPC service

        return Task.CompletedTask;
    }
}
