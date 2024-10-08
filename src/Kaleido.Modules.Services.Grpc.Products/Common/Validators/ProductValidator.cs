using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductValidator : IProductValidator
{
    public async Task ValidateCreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ValidationException("Product Name is required");
        }

        // Product name can be at most 100 characters
        if (product.Name.Length > 100)
        {
            throw new ValidationException("Product Name must be at most 100 characters");
        }

        // if (product.Price < 0)
        // {
        //     throw new ValidationException("Product Price must be greater than or equal to 0");
        // }

        if (string.IsNullOrWhiteSpace(product.CategoryKey))
        {
            throw new ValidationException("Product CategoryId is required");
        }

        // description is not required but if it is provided, it can be at most 500 characters
        if (!string.IsNullOrWhiteSpace(product.Description) && product.Description.Length > 500)
        {
            throw new ValidationException("Product Description must be at most 500 characters");
        }

        await Task.CompletedTask;
    }

    public async Task ValidateUpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(product.Key))
        {
            throw new ValidationException("Product Key is required");
        }

        await ValidateCreateAsync(product, cancellationToken);
    }
}