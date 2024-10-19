using Kaleido.Common.Services.Grpc.Validators;
using Kaleido.Common.Services.Grpc.Models.Validations;
using Kaleido.Grpc.Products;

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
