using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllRevisions;

public interface IGetAllRevisionsHandler : IBaseHandler<ProductRequest, ProductListResponse>
{
}