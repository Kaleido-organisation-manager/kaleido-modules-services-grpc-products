namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public interface IGetAllProductManager
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
}