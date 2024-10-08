namespace Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;

public interface IGetProductPriceRevisionsManager
{
    Task<IEnumerable<ProductPriceRevision>> GetAllAsync(string key, string currency, CancellationToken cancellationToken = default);
}
