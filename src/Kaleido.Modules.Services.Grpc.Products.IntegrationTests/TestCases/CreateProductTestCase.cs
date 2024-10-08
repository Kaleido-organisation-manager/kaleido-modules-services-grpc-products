using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Grpc.Net.Client;
using Kaleido.Modules.Services.Grpc.Products.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Kaleido.Modules.Services.Grpc.Products.IntegrationTests.TestCases;

[TestClass]
public class CreateProductTestCase : IDisposable
{
    private PostgreSqlContainer _dbContainer = null!;
    private IContainer _grpcContainer = null!;

    public void Dispose()
    {
        _grpcContainer.StopAsync().Wait();
        _dbContainer.StopAsync().Wait();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        var postgresPort = 5432;
        var postgresUser = "user";
        var postgresPassword = "password";
        var postgresDatabase = "products";

        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase(postgresDatabase)
            .WithUsername(postgresUser)
            .WithPassword(postgresPassword)
            .WithExposedPort(postgresPort)
            .WithPortBinding(postgresPort, true)
            .WithHostname("localhost")
            .WithWaitStrategy(Wait.ForUnixContainer()
                      .UntilMessageIsLogged("database system is ready to accept connections"))
            // .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await _dbContainer.StartAsync();

        var connectionString = $"Server=localhost;Port={_dbContainer.GetMappedPublicPort(postgresPort)};Database={postgresDatabase};Username={postgresUser};Password={postgresPassword};";
        var connectionStringDocker = $"Server=host.internal.docker;Port={_dbContainer.GetMappedPublicPort(postgresPort)};Database={postgresDatabase};Username={postgresUser};Password={postgresPassword};";

        await WaitForPostgresAsync(connectionString);

        _grpcContainer = new ContainerBuilder()
            .WithImage("kaleido-modules-services-grpc-products:latest")
            .WithPortBinding(8080, true)
            .WithEnvironment("ConnectionStrings:Products", connectionStringDocker)
            .DependsOn(_dbContainer)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
            .Build();

        await _grpcContainer.StartAsync();

        var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:Products"] = connectionString
                        });
                    });
                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<ProductsDbContext>();
                        services.AddDbContext<ProductsDbContext>(options =>
                            options.UseNpgsql(connectionString));
                    });
                });

        // Initialize database
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        dbContext.Database.Migrate();
        await dbContext.Database.CloseConnectionAsync();
    }

    private async Task WaitForPostgresAsync(string connectionString, int timeoutSeconds = 30)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var deadline = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(connectionString);
                await connection.OpenAsync(); // Attempt to open the connection

                // If successful, exit the method
                return;
            }
            catch
            {
                // Ignore exceptions and keep checking
            }

            await Task.Delay(500); // Wait before trying again
        }

        throw new TimeoutException("PostgreSQL server did not start within the expected time.");
    }

    [TestMethod]
    public void CreateProduct()
    {
        var channel = GrpcChannel.ForAddress($"http://localhost:{_grpcContainer.GetMappedPublicPort(8080)}");
        var client = new Client.Products.ProductsClient(channel);

        var requestProduct = new Client.CreateProduct
        {
            Name = "Test Product",
            Description = "Test Description",
            CategoryKey = Guid.NewGuid().ToString(),
            ImageUrl = "https://test.com/image.jpg",
            Prices = { new Client.ProductPrice { CurrencyKey = Guid.NewGuid().ToString(), Value = 10 } }
        };

        var response = client.CreateProduct(new Client.CreateProductRequest
        {
            Product = requestProduct
        });

        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Product);
        Assert.AreEqual(requestProduct.Name, response.Product.Name);
        Assert.AreEqual(requestProduct.Description, response.Product.Description);
        Assert.AreEqual(requestProduct.CategoryKey, response.Product.CategoryKey);
        Assert.AreEqual(requestProduct.ImageUrl, response.Product.ImageUrl);
        Assert.AreEqual(requestProduct.Prices.Count, response.Product.Prices.Count);
        Assert.AreEqual(requestProduct.Prices[0].CurrencyKey, response.Product.Prices[0].CurrencyKey);
        Assert.AreEqual(requestProduct.Prices[0].Value, response.Product.Prices[0].Value);

    }
}