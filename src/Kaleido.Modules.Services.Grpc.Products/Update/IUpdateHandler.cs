using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Update;

public interface IUpdateHandler : IBaseHandler<ProductActionRequest, ProductResponse>
{
}