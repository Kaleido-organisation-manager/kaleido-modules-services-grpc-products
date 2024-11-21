using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Services;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Moq;
using Moq.AutoMock;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.Common.Services;

public class ProductsServiceTests
{
    private readonly AutoMocker _mocker;
    private readonly ProductsService _sut;
    private readonly Product _testProduct;
    private readonly ProductResponse _testResponse;
    private readonly ServerCallContext _testContext;

    public ProductsServiceTests()
    {
        _mocker = new AutoMocker();
        _sut = _mocker.CreateInstance<ProductsService>();

        _testProduct = new Product
        {
            Name = "Test Product",
            CategoryKey = "test-category",
            Description = "Test Description"
        };

        _testResponse = new ProductResponse();

        // Setup happy path for CreateHandler
        _mocker.GetMock<ICreateHandler>()
            .Setup(x => x.HandleAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testResponse);

        // Mock ServerCallContext since it's sealed
        _testContext = TestServerCallContext.Create();
    }

    [Fact]
    public async Task CreateProduct_ShouldCallCreateHandler()
    {
        // Act
        var result = await _sut.CreateProduct(_testProduct, _testContext);

        // Assert
        _mocker.GetMock<ICreateHandler>()
            .Verify(x => x.HandleAsync(_testProduct, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(_testResponse, result);
    }
}

// Helper class to create ServerCallContext for testing
public class TestServerCallContext : ServerCallContext
{
    private TestServerCallContext()
    {
    }

    public static ServerCallContext Create()
    {
        return new TestServerCallContext();
    }

    protected override string MethodCore => "TestMethod";
    protected override string HostCore => "TestHost";
    protected override string PeerCore => "TestPeer";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore => new Metadata();
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore => new Metadata();
    protected override Status StatusCore { get; set; } = Status.DefaultSuccess;
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