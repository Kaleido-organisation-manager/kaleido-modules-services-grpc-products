namespace Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;

public interface ICategoryValidator
{
    Task ValidateIdAsync(string id, CancellationToken cancellationToken = default);
}