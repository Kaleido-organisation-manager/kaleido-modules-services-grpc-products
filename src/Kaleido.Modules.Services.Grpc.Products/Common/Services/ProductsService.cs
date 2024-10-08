using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Services;

public class ProductsService : Products.ProductsBase
{
    private readonly ILogger<ProductsService> _logger;
    private readonly IBaseHandler<GetProductRequest, GetProductResponse> _getProductHandler;
    private readonly IBaseHandler<GetAllProductsRequest, GetAllProductsResponse> _getAllProductsHandler;
    private readonly IBaseHandler<GetAllProductsByCategoryIdRequest, GetAllProductsResponse> _getAllProductsByCategoryIdHandler;
    private readonly IBaseHandler<CreateProductRequest, CreateProductResponse> _createProductHandler;
    private readonly IBaseHandler<UpdateProductRequest, UpdateProductResponse> _updateProductHandler;
    private readonly IBaseHandler<DeleteProductRequest, DeleteProductResponse> _deleteProductHandler;
    private readonly IBaseHandler<GetProductRequest, GetProductRevisionsResponse> _getProductRevisionsHandler;
    private readonly IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse> _getProductRevisionHandler;
    private readonly IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse> _getProductPriceRevisionsHandler;
    private readonly IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse> _getProductPriceRevisionHandler;

    public ProductsService(
        ILogger<ProductsService> logger,
        IBaseHandler<GetProductRequest, GetProductResponse> getProductHandler,
        IBaseHandler<GetAllProductsRequest, GetAllProductsResponse> getAllProductsHandler,
        IBaseHandler<GetAllProductsByCategoryIdRequest, GetAllProductsResponse> getAllProductsByCategoryIdHandler,
        IBaseHandler<CreateProductRequest, CreateProductResponse> createProductHandler,
        IBaseHandler<UpdateProductRequest, UpdateProductResponse> updateProductHandler,
        IBaseHandler<DeleteProductRequest, DeleteProductResponse> deleteProductHandler,
        IBaseHandler<GetProductRequest, GetProductRevisionsResponse> getProductRevisionsHandler,
        IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse> getProductRevisionHandler,
        IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse> getProductPriceRevisionsHandler,
        IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse> getProductPriceRevisionHandler)
    {
        _logger = logger;
        _getProductHandler = getProductHandler;
        _getAllProductsHandler = getAllProductsHandler;
        _getAllProductsByCategoryIdHandler = getAllProductsByCategoryIdHandler;
        _createProductHandler = createProductHandler;
        _updateProductHandler = updateProductHandler;
        _deleteProductHandler = deleteProductHandler;
        _getProductRevisionsHandler = getProductRevisionsHandler;
        _getProductRevisionHandler = getProductRevisionHandler;
        _getProductPriceRevisionsHandler = getProductPriceRevisionsHandler;
        _getProductPriceRevisionHandler = getProductPriceRevisionHandler;
    }

    public override async Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProduct with key: {Key}", request.Key);
        return await _getProductHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetAllProductsResponse> GetAllProducts(GetAllProductsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProducts");
        return await _getAllProductsHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetAllProductsResponse> GetAllProductsByCategoryId(GetAllProductsByCategoryIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProductsByCategoryId with CategoryId: {CategoryId}", request.CategoryKey);
        return await _getAllProductsByCategoryIdHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<CreateProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for CreateProduct with name: {Name}", request.Product.Name);
        return await _createProductHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<UpdateProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for UpdateProduct with key: {Key}", request.Key);
        return await _updateProductHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for DeleteProduct with key: {Key}", request.Key);
        return await _deleteProductHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetProductRevisionsResponse> GetProductRevisions(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProductRevisions with key: {Key}", request.Key);
        return await _getProductRevisionsHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetProductRevisionResponse> GetProductRevision(GetProductRevisionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProductRevision with key: {Key} and revision: {Revision}", request.Key, request.Revision);
        return await _getProductRevisionHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetProductPriceRevisionsResponse> GetProductPriceRevisions(GetProductPriceRevisionsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProductPriceRevisions with key: {Key}", request.Key);
        return await _getProductPriceRevisionsHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetProductPriceRevisionResponse> GetProductPriceRevision(GetProductPriceRevisionRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetProductPriceRevision with key: {Key}, revision: {Revision}, and currencyKey: {CurrencyKey}", request.Key, request.Revision, request.CurrencyKey);
        return await _getProductPriceRevisionHandler.HandleAsync(request, context.CancellationToken);
    }
}