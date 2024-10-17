using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllByName;

public class GetAllByNameRequestValidator : IRequestValidator<GetAllProductsByNameRequest>
{
    public Task<ValidationResult> ValidateAsync(GetAllProductsByNameRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            validationResult.AddInvalidFormatError([nameof(request.Name)], "Name is required");
        }

        return Task.FromResult(validationResult);
    }
}
