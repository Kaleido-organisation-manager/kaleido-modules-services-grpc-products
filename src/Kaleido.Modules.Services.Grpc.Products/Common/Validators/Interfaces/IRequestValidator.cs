using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface IRequestValidator<T>
{
    Task<ValidationResult> ValidateAsync(T request, CancellationToken cancellationToken = default);
}
