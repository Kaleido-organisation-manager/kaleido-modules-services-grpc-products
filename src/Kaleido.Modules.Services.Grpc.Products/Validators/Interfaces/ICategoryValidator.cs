namespace Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;

public interface ICategoryValidator
{
    Task ValidateIdAsync(Guid id, CancellationToken cancellationToken = default);
}