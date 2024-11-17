using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public interface IGetHandler : IBaseHandler<ProductRequest, ProductResponse>
{
}