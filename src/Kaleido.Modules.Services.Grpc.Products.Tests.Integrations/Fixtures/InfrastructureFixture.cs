using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Testcontainers.PostgreSql;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures
{
    public class InfrastructureFixture : IDisposable
    {
        private const int TIMEOUT_WAIT_MINUTES = 1;
        private const string NETWORK_NAME = "kaleido-modules-services-grpc-products-integration-tests";
        private const string DB_NAME = "products";
        private const string DB_USER = "postgres";
        private const string DB_PASSWORD = "postgres";

        private string _migrationImageName = "kaleido-modules-services-grpc-products-migrations:latest";
        private string _grpcImageName = "kaleido-modules-services-grpc-products:latest";
        private readonly bool _isLocalDevelopment;
        private IFutureDockerImage? _grpcImage;
        private IFutureDockerImage? _migrationImage;
        private IContainer _migrationContainer = null!;
        private PostgreSqlContainer _postgres { get; }
        private GrpcChannel _channel { get; set; } = null!;

        public GrpcProductsClient Client { get; private set; } = null!;
        public IContainer GrpcContainer { get; private set; } = null!;
        public string ConnectionString { get; private set; } = null!;

        public InfrastructureFixture()
        {
            // Read from environment variables or appsettings.json file
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

            _isLocalDevelopment = configuration.GetValue<bool?>("CI") == null;

            if (!_isLocalDevelopment)
            {
                _grpcImageName = configuration.GetValue<string>("PRODUCTS_IMAGE_NAME") ?? _grpcImageName;
                _migrationImageName = configuration.GetValue<string>("MIGRATIONS_IMAGE_NAME") ?? _migrationImageName;
            }

            _postgres = new PostgreSqlBuilder()
                .WithDatabase(DB_NAME)
                .WithUsername(DB_USER)
                .WithPassword(DB_PASSWORD)
                .WithLogger(new LoggerFactory().CreateLogger<PostgreSqlContainer>())
                .WithPortBinding(5432, true)
                .WithExposedPort(5432)
                .WithNetworkAliases("postgres")
                .WithHostname("postgres")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("database system is ready to accept connections"))
                .Build();

            if (_isLocalDevelopment)
            {
                var nugetUser = configuration.GetValue<string>("NUGET_USER");
                var nugetToken = configuration.GetValue<string>("NUGET_TOKEN");

                _migrationImage = new ImageFromDockerfileBuilder()
                    .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                    .WithDockerfile("dockerfiles/Grpc.Products.Migrations/Dockerfile.local")
                    .WithName(_migrationImageName)
                    .WithLogger(new LoggerFactory().CreateLogger<ImageFromDockerfileBuilder>())
                    .WithCleanUp(false)
                    .WithBuildArgument("NUGET_USER", nugetUser)
                    .WithBuildArgument("NUGET_TOKEN", nugetToken)
                    .Build();

                _grpcImage = new ImageFromDockerfileBuilder()
                    .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                    .WithDockerfile("dockerfiles/Grpc.Products/Dockerfile.local")
                    .WithName(_grpcImageName)
                    .WithLogger(new LoggerFactory().CreateLogger<ImageFromDockerfileBuilder>())
                    .WithBuildArgument("NUGET_USER", nugetUser)
                    .WithBuildArgument("NUGET_TOKEN", nugetToken)
                    .WithCleanUp(false)
                    .Build();
            }

            InitializeAsync().Wait();
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));
            await _postgres.WaitForPort().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));

            if (_migrationImage != null)
            {
                await _migrationImage.CreateAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));
            }

            if (_grpcImage != null)
            {
                await _grpcImage.CreateAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));
            }


            string host = "host.testcontainers.internal";
            var postgresPort = _postgres.GetMappedPublicPort(5432);
            ConnectionString = $"Server={host};Port={postgresPort};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";
            await TestcontainersSettings.ExposeHostPortsAsync(postgresPort)
                .ConfigureAwait(false);

            _migrationContainer = new ContainerBuilder()
                .WithImage(_migrationImageName)
                .WithEnvironment("ConnectionStrings:Products", ConnectionString)
                .DependsOn(_postgres)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Migration completed successfully."))
                .WithLogger(new LoggerFactory().CreateLogger<IContainer>())
                .Build();

            await _migrationContainer.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));

            GrpcContainer = new ContainerBuilder()
                .WithImage(_grpcImageName)
                .WithPortBinding(8080, true)
                .WithExposedPort(8080)
                .DependsOn(_migrationContainer)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
                .WithEnvironment("ConnectionStrings:Products", ConnectionString)
                .WithLogger(new LoggerFactory().CreateLogger<IContainer>())
                .Build();

            await GrpcContainer.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));


            var grpcPort = GrpcContainer.GetMappedPublicPort(8080);
            await TestcontainersSettings.ExposeHostPortsAsync(grpcPort)
                .ConfigureAwait(false);
            var grpcUri = new UriBuilder("http", GrpcContainer.Hostname, grpcPort);
            _channel = GrpcChannel.ForAddress(grpcUri.Uri);

            Client = new GrpcProductsClient(_channel);
        }

        public async Task DisposeAsync()
        {
            await _migrationContainer.DisposeAsync();
            await _postgres.DisposeAsync();
            _channel.Dispose();
            await GrpcContainer.DisposeAsync();
        }

        public void Dispose()
        {
            DisposeAsync().Wait();
        }
    }
}