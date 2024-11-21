
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAll;

public interface IGetAllHandler : IBaseHandler<EmptyRequest, ProductListResponse>
{
}