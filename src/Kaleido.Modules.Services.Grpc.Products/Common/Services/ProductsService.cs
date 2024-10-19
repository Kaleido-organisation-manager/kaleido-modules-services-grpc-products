using Grpc.Core;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Grpc.Products;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Services;

public class ProductsService : GrpcProducts.GrpcProductsBase
{
    private readonly IBaseHandler<CreateProductRequest, CreateProductResponse> _createProductHandler;
    private readonly IBaseHandler<DeleteProductRequest, DeleteProductResponse> _deleteProductHandler;
    private readonly IBaseHandler<GetAllProductsByCategoryKeyRequest, GetAllProductsByCategoryKeyResponse> _getAllProductsByCategoryKeyHandler;
    private readonly IBaseHandler<GetAllProductsByNameAndCategoryKeyRequest, GetAllProductsByNameAndCategoryKeyResponse> _getAllProductsByNameAndCategoryKeyHandler;
    private readonly IBaseHandler<GetAllProductsByNameRequest, GetAllProductsByNameResponse> _getAllProductsByNameHandler;
    private readonly IBaseHandler<GetAllProductsRequest, GetAllProductsResponse> _getAllProductsHandler;
    private readonly IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse> _getProductPriceRevisionHandler;
    private readonly IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse> _getProductPriceRevisionsHandler;
    private readonly IBaseHandler<GetProductRequest, GetProductResponse> _getProductHandler;
    private readonly IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse> _getProductRevisionHandler;
    private readonly IBaseHandler<GetProductRevisionsRequest, GetProductRevisionsResponse> _getProductRevisionsHandler;
    private readonly IBaseHandler<UpdateProductRequest, UpdateProductResponse> _updateProductHandler;
    private readonly ILogger<ProductsService> _logger;

    public ProductsService(
        IBaseHandler<CreateProductRequest, CreateProductResponse> createProductHandler,
        IBaseHandler<DeleteProductRequest, DeleteProductResponse> deleteProductHandler,
        IBaseHandler<GetAllProductsByCategoryKeyRequest, GetAllProductsByCategoryKeyResponse> getAllProductsByCategoryKeyHandler,
        IBaseHandler<GetAllProductsByNameAndCategoryKeyRequest, GetAllProductsByNameAndCategoryKeyResponse> getAllProductsByNameAndCategoryKeyHandler,
        IBaseHandler<GetAllProductsByNameRequest, GetAllProductsByNameResponse> getAllProductsByNameHandler,
        IBaseHandler<GetAllProductsRequest, GetAllProductsResponse> getAllProductsHandler,
        IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse> getProductPriceRevisionHandler,
        IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse> getProductPriceRevisionsHandler,
        IBaseHandler<GetProductRequest, GetProductResponse> getProductHandler,
        IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse> getProductRevisionHandler,
        IBaseHandler<GetProductRevisionsRequest, GetProductRevisionsResponse> getProductRevisionsHandler,
        IBaseHandler<UpdateProductRequest, UpdateProductResponse> updateProductHandler,
        ILogger<ProductsService> logger
        )
    {
        _createProductHandler = createProductHandler;
        _deleteProductHandler = deleteProductHandler;
        _getAllProductsByCategoryKeyHandler = getAllProductsByCategoryKeyHandler;
        _getAllProductsByNameAndCategoryKeyHandler = getAllProductsByNameAndCategoryKeyHandler;
        _getAllProductsByNameHandler = getAllProductsByNameHandler;
        _getAllProductsHandler = getAllProductsHandler;
        _getProductHandler = getProductHandler;
        _getProductPriceRevisionHandler = getProductPriceRevisionHandler;
        _getProductPriceRevisionsHandler = getProductPriceRevisionsHandler;
        _getProductRevisionHandler = getProductRevisionHandler;
        _getProductRevisionsHandler = getProductRevisionsHandler;
        _logger = logger;
        _updateProductHandler = updateProductHandler;
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

    public override async Task<GetAllProductsByCategoryKeyResponse> GetAllProductsByCategoryKey(GetAllProductsByCategoryKeyRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProductsByCategoryKey with CategoryKey: {CategoryKey}", request.CategoryKey);
        return await _getAllProductsByCategoryKeyHandler.HandleAsync(request, context.CancellationToken);
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

    public override async Task<GetProductRevisionsResponse> GetProductRevisions(GetProductRevisionsRequest request, ServerCallContext context)
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

    public override async Task<GetAllProductsByNameResponse> GetAllProductsByName(GetAllProductsByNameRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProductsByName with name: {Name}", request.Name);
        return await _getAllProductsByNameHandler.HandleAsync(request, context.CancellationToken);
    }

    public override async Task<GetAllProductsByNameAndCategoryKeyResponse> GetAllProductsByNameAndCategoryKey(GetAllProductsByNameAndCategoryKeyRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC request received for GetAllProductsByNameAndCategoryKey with name: {Name} and categoryKey: {CategoryKey}", request.Name, request.CategoryKey);
        return await _getAllProductsByNameAndCategoryKeyHandler.HandleAsync(request, context.CancellationToken);
    }
}
