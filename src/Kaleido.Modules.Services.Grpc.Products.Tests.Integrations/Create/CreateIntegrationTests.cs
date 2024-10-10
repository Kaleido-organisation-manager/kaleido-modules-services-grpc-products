using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Grpc.Net.Client;
using Kaleido.Grpc.Products;
using Testcontainers.PostgreSql;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Create;

public class CreateIntegrationTests : IAsyncLifetime
{
    private const string MIGRATION_IMAGE_NAME = "kaleido-modules-services-grpc-products-migrations:latest";
    private const string GRPC_IMAGE_NAME = "kaleido-modules-services-grpc-products:latest";

    private const string DB_NAME = "products";
    private const string DB_USER = "postgres";
    private const string DB_PASSWORD = "postgres";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase(DB_NAME)
        .WithUsername(DB_USER)
        .WithPassword(DB_PASSWORD)
        .WithHostname("localhost")
        .WithExtraHost("host.docker.internal", "host-gateway")
        .Build();

    private readonly IFutureDockerImage _migrationImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
        .WithDockerfile("docker/Grpc.Products.Migrations/Dockerfile")
        .WithName(MIGRATION_IMAGE_NAME)
        .Build();

    private readonly IFutureDockerImage _grpcImage = new ImageFromDockerfileBuilder()
        .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
        .WithDockerfile("docker/Grpc.Products/Dockerfile")
        .WithName(GRPC_IMAGE_NAME)
        .Build();

    private IContainer _migrationContainer = null!;
    private IContainer _grpcContainer = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _postgres.WaitForPort();

        await _migrationImage.CreateAsync();
        await _grpcImage.CreateAsync();


        var dbConnectionString = $"Server=host.docker.internal;Port={_postgres.GetMappedPublicPort(5432)};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";
        Console.WriteLine($"ConnectionString:Products: {dbConnectionString}");

        _migrationContainer = new ContainerBuilder()
            .WithImage(MIGRATION_IMAGE_NAME)
            .WithEnvironment("ConnectionStrings:Products", dbConnectionString)
            .DependsOn(_postgres)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Migration completed successfully."))
            .Build();

        _grpcContainer = new ContainerBuilder()
            .WithImage(GRPC_IMAGE_NAME)
            .WithPortBinding(8080, true)
            .WithExposedPort(8080)
            .WithEnvironment("ConnectionStrings:Products", dbConnectionString)
            .Build();

        await _migrationContainer.StartAsync();
        await _grpcContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _migrationContainer.StopAsync();
        await _grpcContainer.StopAsync();
        await _postgres.StopAsync();
    }

    [Fact]
    public async Task Create_ShouldCreateProduct()
    {
        var grpcConnectionString = $"http://{_grpcContainer.Hostname}:{_grpcContainer.GetMappedPublicPort(8080)}";
        var channel = GrpcChannel.ForAddress(grpcConnectionString);
        var client = new GrpcProducts.GrpcProductsClient(channel);

        var request = new CreateProductRequest
        {
            Product = new CreateProduct
            {
                Name = "Test Product",
                CategoryKey = Guid.NewGuid().ToString(),
                Description = "Test Product Description",
                ImageUrl = "https://test.com/image.jpg",
                Prices = { new List<ProductPrice> {
                    new ProductPrice
                    {
                        CurrencyKey = Guid.NewGuid().ToString(),
                        Value = 100.00f
                    }
                }}
            }
        };

        var response = await client.CreateProductAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Product);
        Assert.NotNull(response.Product.Key);
    }
}
