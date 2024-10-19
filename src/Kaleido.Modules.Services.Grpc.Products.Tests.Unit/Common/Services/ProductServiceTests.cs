using Moq;
using Moq.AutoMock;
using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Kaleido.Common.Services.Grpc.Handlers;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Services;

public class ProductsServiceTests
{
    private readonly AutoMocker _mocker;
    private readonly ProductsService _productsService;

    public ProductsServiceTests()
    {
        _mocker = new AutoMocker();
        _mocker.Use<ILogger<ProductsService>>(NullLogger<ProductsService>.Instance);
        _productsService = _mocker.CreateInstance<ProductsService>();
    }

    [Fact]
    public async Task GetProduct_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetProductRequest()
        {
            Key = Guid.NewGuid().ToString()
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetProduct(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetProductRequest, GetProductResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAllProducts_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetAllProductsRequest();
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetAllProducts(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetAllProductsRequest, GetAllProductsResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsByCategoryKey_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetAllProductsByCategoryKeyRequest()
        {
            CategoryKey = Guid.NewGuid().ToString()
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetAllProductsByCategoryKey(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetAllProductsByCategoryKeyRequest, GetAllProductsByCategoryKeyResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new CreateProductRequest()
        {
            Product = new CreateProduct()
            {
                Name = "Test Product",
                Description = "Test Description",
                CategoryKey = Guid.NewGuid().ToString(),
                ImageUrl = "https://test.com/image.jpg",
            }
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.CreateProduct(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<CreateProductRequest, CreateProductResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new UpdateProductRequest()
        {
            Key = Guid.NewGuid().ToString(),
            Product = new Product()
            {
                Name = "Test Product",
                Description = "Test Description",
                CategoryKey = Guid.NewGuid().ToString(),
                ImageUrl = "https://test.com/image.jpg",
            }
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.UpdateProduct(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<UpdateProductRequest, UpdateProductResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new DeleteProductRequest()
        {
            Key = Guid.NewGuid().ToString()
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.DeleteProduct(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<DeleteProductRequest, DeleteProductResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetProductRevisions_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetProductRevisionsRequest()
        {
            Key = Guid.NewGuid().ToString()
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetProductRevisions(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetProductRevisionsRequest, GetProductRevisionsResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetProductRevision_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetProductRevisionRequest()
        {
            Key = Guid.NewGuid().ToString(),
            Revision = 1
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetProductRevision(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetProductPriceRevisions_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetProductPriceRevisionsRequest()
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString()
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetProductPriceRevisions(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetProductPriceRevision_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetProductPriceRevisionRequest()
        {
            Key = Guid.NewGuid().ToString(),
            CurrencyKey = Guid.NewGuid().ToString(),
            Revision = 1
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetProductPriceRevision(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsByName_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetAllProductsByNameRequest()
        {
            Name = "Test Product"
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetAllProductsByName(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetAllProductsByNameRequest, GetAllProductsByNameResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAllProductsByNameAndCategoryKey_ShouldCallCorrectHandler()
    {
        // Arrange
        var request = new GetAllProductsByNameAndCategoryKeyRequest()
        {
            Name = "Test Product",
            CategoryKey = Guid.NewGuid().ToString()
        };
        var context = TestServerCallContext.Create();

        // Act
        await _productsService.GetAllProductsByNameAndCategoryKey(request, context);

        // Assert
        _mocker.GetMock<IBaseHandler<GetAllProductsByNameAndCategoryKeyRequest, GetAllProductsByNameAndCategoryKeyResponse>>()
            .Verify(h => h.HandleAsync(request, context.CancellationToken), Times.Once);
    }
}

// Helper class for creating a test ServerCallContext
public class TestServerCallContext : ServerCallContext
{
    private readonly CancellationToken _cancellationToken;

    private TestServerCallContext(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public static TestServerCallContext Create(CancellationToken cancellationToken = default)
    {
        return new TestServerCallContext(cancellationToken);
    }

    protected override string MethodCore => "TestMethod";
    protected override string HostCore => "TestHost";
    protected override string PeerCore => "TestPeer";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => new Metadata();
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override Metadata ResponseTrailersCore => new Metadata();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore => throw new NotImplementedException();

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
    {
        throw new NotImplementedException();
    }

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }
}

