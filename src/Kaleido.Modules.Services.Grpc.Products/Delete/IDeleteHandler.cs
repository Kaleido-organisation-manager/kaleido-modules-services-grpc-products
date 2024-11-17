using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public interface IDeleteHandler : IBaseHandler<ProductRequest, ProductResponse>
{
}
