namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public interface IDeleteManager
{
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}