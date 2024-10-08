namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface ICategoryValidator
{
    Task ValidateIdAsync(Guid id, CancellationToken cancellationToken = default);
}