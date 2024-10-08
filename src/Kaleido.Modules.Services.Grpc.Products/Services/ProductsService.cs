using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Services;

public class ProductsService : Products.ProductsBase
{
    private readonly ICreateProductHandler _createProductHandler;
    private readonly IDeleteProductHandler _deleteProductHandler;
    private readonly IGetAllProductsByCategoryIdHandler _getAllProductsByCategoryIdHandler;
    private readonly IGetAllProductsHandler _getAllProductsHandler;
    private readonly IGetProductHandler _getProductHandler;
    private readonly IGetProductRevisionHandler _getProductRevisionHandler;
    private readonly IGetProductRevisionsHandler _getProductRevisionsHandler;
    private readonly ILogger<ProductsService> _logger;
    private readonly IUpdateProductHandler _updateProductHandler;

    public ProductsService(
            ICreateProductHandler createProductHandler,
            IDeleteProductHandler deleteProductHandler,
            IGetAllProductsByCategoryIdHandler getAllProductsByCategoryIdHandler,
            IGetAllProductsHandler getAllProductsHandler,
            IGetProductHandler getProductHandler,
            IGetProductRevisionHandler getProductRevisionHandler,
            IGetProductRevisionsHandler getProductRevisionsHandler,
            ILogger<ProductsService> logger,
            IUpdateProductHandler updateProductHandler
        )
    {
        _createProductHandler = createProductHandler;
        _deleteProductHandler = deleteProductHandler;
        _getAllProductsByCategoryIdHandler = getAllProductsByCategoryIdHandler;
        _getAllProductsHandler = getAllProductsHandler;
        _getProductHandler = getProductHandler;
        _getProductRevisionHandler = getProductRevisionHandler;
        _getProductRevisionsHandler = getProductRevisionsHandler;
        _logger = logger;
        _updateProductHandler = updateProductHandler;
    }

    public override async Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProduct with key: {Key}", request.Key);
        return await _getProductHandler.HandleAsync(request.Key, context.CancellationToken);
    }

    public override async Task<GetAllProductsResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProducts");
        return await _getAllProductsHandler.HandleAsync(context.CancellationToken);
    }

    public override async Task<GetAllProductsResponse> GetAllProductsByCategoryId(GetAllProductsByCategoryIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProductsByCategoryId with CategoryId: {CategoryId}", request.CategoryKey);
        return await _getAllProductsByCategoryIdHandler.HandleAsync(request.CategoryKey, context.CancellationToken);
    }

    public override async Task<CreateProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for CreateProduct with name: {Name}", request.Product.Name);
        return await _createProductHandler.HandleAsync(request.Product, context.CancellationToken);
    }

    public override async Task<UpdateProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for UpdateProduct with key: {Key}", request.Key);
        return await _updateProductHandler.HandleAsync(request.Key, request.Product, context.CancellationToken);
    }

    public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for DeleteProduct with key: {Key}", request.Key);
        return await _deleteProductHandler.HandleAsync(request.Key, context.CancellationToken);
    }

    public override async Task<GetProductRevisionsResponse> GetProductRevisions(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProductRevisions with key: {Key}", request.Key);
        return await _getProductRevisionsHandler.HandleAsync(request.Key, context.CancellationToken);
    }

    public override async Task<GetProductRevisionResponse> GetProductRevision(GetProductRevisionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProductRevision with key: {Key} and revision: {Revision}", request.Key, request.Revision);
        return await _getProductRevisionHandler.HandleAsync(request.Key, request.Revision, context.CancellationToken);
    }

}