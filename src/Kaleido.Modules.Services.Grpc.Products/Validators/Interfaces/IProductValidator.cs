namespace Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;

public interface IProductValidator
{
    Task ValidateCreateAsync(Product product, CancellationToken cancellationToken = default);
    Task ValidateUpdateAsync(Product product, CancellationToken cancellationToken = default);
}