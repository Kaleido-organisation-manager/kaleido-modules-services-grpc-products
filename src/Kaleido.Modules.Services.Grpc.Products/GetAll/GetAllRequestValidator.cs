using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Grpc.Products;

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