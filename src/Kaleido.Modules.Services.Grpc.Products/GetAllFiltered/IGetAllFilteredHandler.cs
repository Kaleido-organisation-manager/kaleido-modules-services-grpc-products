using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;

public interface IGetAllFilteredHandler : IBaseHandler<ProductFilterRequest, ProductListResponse>
{
}