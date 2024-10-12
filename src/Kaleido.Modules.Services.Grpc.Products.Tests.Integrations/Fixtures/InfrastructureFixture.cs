using DotNet.Testcontainers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Grpc.Net.Client;
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
        private INetwork _network = null!;

        public GrpcProductsClient Client { get; private set; } = null!;
        public IContainer GrpcContainer { get; private set; } = null!;
        public string ConnectionString { get; private set; } = null!;

        public InfrastructureFixture()
        {
            _isLocalDevelopment = Environment.GetEnvironmentVariable("CI") == null;

            if (!_isLocalDevelopment)
            {
                _grpcImageName = Environment.GetEnvironmentVariable("PRODUCTS_IMAGE_NAME") ?? _grpcImageName;
                _migrationImageName = Environment.GetEnvironmentVariable("MIGRATIONS_IMAGE_NAME") ?? _migrationImageName;
            }

            _network = new NetworkBuilder()
                .WithName(NETWORK_NAME)
                .WithDockerEndpoint(Environment.GetEnvironmentVariable("DOCKER_HOST") ?? "unix:///var/run/docker.sock")
                .Build();

            _postgres = new PostgreSqlBuilder()
                .WithDatabase(DB_NAME)
                .WithUsername(DB_USER)
                .WithPassword(DB_PASSWORD)
                .WithLogger(new LoggerFactory().CreateLogger<PostgreSqlContainer>())
                .WithPortBinding(5432, true)
                .WithExposedPort(5432)
                .WithNetwork(_network)
                .WithNetworkAliases("postgres")
                .WithHostname("postgres")
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilMessageIsLogged("database system is ready to accept connections"))
                .Build();

            if (_isLocalDevelopment)
            {
                _migrationImage = new ImageFromDockerfileBuilder()
                    .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                    .WithDockerfile("dockerfiles/Grpc.Products.Migrations/Dockerfile")
                    .WithName(_migrationImageName)
                    .WithLogger(new LoggerFactory().CreateLogger<ImageFromDockerfileBuilder>())
                    .WithCleanUp(false)
                    .Build();

                _grpcImage = new ImageFromDockerfileBuilder()
                    .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                    .WithDockerfile("dockerfiles/Grpc.Products/Dockerfile")
                    .WithName(_grpcImageName)
                    .WithLogger(new LoggerFactory().CreateLogger<ImageFromDockerfileBuilder>())
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


            string host = "host.docker.internal";
            ConnectionString = $"Server={host};Port={_postgres.GetMappedPublicPort(5432)};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";
            Console.WriteLine(ConnectionString);

            _migrationContainer = new ContainerBuilder()
                .WithImage(_migrationImageName)
                .WithEnvironment("ConnectionStrings:Products", ConnectionString)
                .DependsOn(_postgres)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Migration completed successfully."))
                .WithLogger(new LoggerFactory().CreateLogger<IContainer>())
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
                .WithNetwork(_network)
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
                .WithNetwork(_network)
                .Build();

            await GrpcContainer.StartAsync().WaitAsync(TimeSpan.FromMinutes(TIMEOUT_WAIT_MINUTES));

            var grpcUri = new UriBuilder("tcp", GrpcContainer.Hostname, GrpcContainer.GetMappedPublicPort(8080));
            _channel = GrpcChannel.ForAddress(grpcUri.Uri);
            // var grpcConnectionString = $"http://{GrpcContainer.Hostname}:{GrpcContainer.GetMappedPublicPort(8080)}";
            // _channel = GrpcChannel.ForAddress(grpcConnectionString);

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