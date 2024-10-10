using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public class GetAllRequestValidator : IRequestValidator<GetAllProductsRequest>
{

    public async Task<ValidationResult> ValidateAsync(GetAllProductsRequest request, CancellationToken cancellationToken = default)
    {
        // For now, we don't need to validate anything
        var validationResult = new ValidationResult();

        return await Task.FromResult(validationResult);
    }
}