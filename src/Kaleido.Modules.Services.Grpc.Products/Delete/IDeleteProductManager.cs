namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public interface IDeleteProductManager
{
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}