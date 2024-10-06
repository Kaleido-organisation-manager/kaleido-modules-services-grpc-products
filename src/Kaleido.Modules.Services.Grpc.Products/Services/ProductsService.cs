using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Services;

public class ProductsService : Products.ProductsBase
{
    private readonly ICreateProductHandler _createProductHandler;
    private readonly IGetAllProductsByCategoryIdHandler _getAllProductsByCategoryIdHandler;
    private readonly IGetAllProductsHandler _getAllProductsHandler;
    private readonly IGetProductHandler _getProductHandler;
    private readonly ILogger<ProductsService> _logger;
    private readonly IUpdateProductHandler _updateProductHandler;

    public ProductsService(
            ICreateProductHandler createProductHandler,
            IGetAllProductsByCategoryIdHandler getAllProductsByCategoryIdHandler,
            IGetAllProductsHandler getAllProductsHandler,
            IGetProductHandler getProductHandler,
            ILogger<ProductsService> logger,
            IUpdateProductHandler updateProductHandler
        )
    {
        _createProductHandler = createProductHandler;
        _getAllProductsByCategoryIdHandler = getAllProductsByCategoryIdHandler;
        _getAllProductsHandler = getAllProductsHandler;
        _getProductHandler = getProductHandler;
        _logger = logger;
        _updateProductHandler = updateProductHandler;
    }

    public override async Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProduct with key: {Id}", request.Id);
        return await _getProductHandler.HandleAsync(request.Id, context.CancellationToken);
    }

    public override async Task<GetAllProductsResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProducts");
        return await _getAllProductsHandler.HandleAsync(context.CancellationToken);
    }

    public override async Task<GetAllProductsResponse> GetAllProductsByCategoryId(GetAllProductsByCategoryIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProductsByCategoryId with CategoryId: {CategoryId}", request.CategoryId);
        return await _getAllProductsByCategoryIdHandler.HandleAsync(request.CategoryId, context.CancellationToken);
    }

    public override async Task<CreateProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for CreateProduct with key: {Id}", request.Product.Name);
        return await _createProductHandler.HandleAsync(request.Product, context.CancellationToken);
    }

    public override async Task<UpdateProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for UpdateProduct with key: {Id}", request.Product.Name);
        return await _updateProductHandler.HandleAsync(request.Product, context.CancellationToken);
    }

}