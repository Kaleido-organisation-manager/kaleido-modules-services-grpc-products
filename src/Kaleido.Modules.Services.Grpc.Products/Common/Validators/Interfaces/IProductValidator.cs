using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface IProductValidator
{
    Task ValidateCreateAsync(Product product, CancellationToken cancellationToken = default);
    Task ValidateUpdateAsync(Product product, CancellationToken cancellationToken = default);
}