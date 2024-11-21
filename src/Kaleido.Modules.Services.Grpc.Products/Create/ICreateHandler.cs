using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public interface ICreateHandler : IBaseHandler<Product, ProductResponse>
{
}