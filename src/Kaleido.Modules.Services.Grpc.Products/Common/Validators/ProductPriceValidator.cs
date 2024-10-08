using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators;

public class ProductPriceValidator : IProductPriceValidator
{

    public Task ValidateAsync(IEnumerable<ProductPrice> productPrices, CancellationToken cancellationToken = default)
    {
        foreach (var productPrice in productPrices)
        {
            if (productPrice.Value <= 0)
            {
                throw new ValidationException("Price must be greater than 0");
            }
        }

        // TODO: Add validation for currency Id

        return Task.CompletedTask;
    }
}