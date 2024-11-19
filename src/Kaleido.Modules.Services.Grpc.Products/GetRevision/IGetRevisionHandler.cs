using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetRevision;

public interface IGetRevisionHandler : IBaseHandler<ProductRevisionRequest, ProductResponse>
{
}