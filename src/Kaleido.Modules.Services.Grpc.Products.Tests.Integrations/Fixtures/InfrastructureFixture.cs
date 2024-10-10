using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Grpc.Net.Client;
using Testcontainers.PostgreSql;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures
{
    public class InfrastructureFixture : IAsyncLifetime
    {
        private const string MIGRATION_IMAGE_NAME = "kaleido-modules-services-grpc-products-migrations:latest";
        private const string GRPC_IMAGE_NAME = "kaleido-modules-services-grpc-products:latest";
        private const string DB_NAME = "products";
        private const string DB_USER = "postgres";
        private const string DB_PASSWORD = "postgres";

        private IFutureDockerImage _grpcImage;
        private IFutureDockerImage _migrationImage;
        private IContainer _migrationContainer = null!;
        private PostgreSqlContainer _postgres { get; }

        public GrpcChannel Channel { get; private set; } = null!;
        public IContainer GrpcContainer { get; private set; } = null!;
        public string ConnectionString { get; private set; } = null!;

        public InfrastructureFixture()
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase(DB_NAME)
                .WithUsername(DB_USER)
                .WithPassword(DB_PASSWORD)
                .WithHostname("localhost")
                .WithExtraHost("host.docker.internal", "host-gateway")
                .Build();

            _migrationImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                .WithDockerfile("docker/Grpc.Products.Migrations/Dockerfile")
                .WithName(MIGRATION_IMAGE_NAME)
                .Build();

            _grpcImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(Path.Join(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath, "../"))
                .WithDockerfile("docker/Grpc.Products/Dockerfile")
                .WithName(GRPC_IMAGE_NAME)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
            await _postgres.WaitForPort();

            await _migrationImage.CreateAsync();
            await _grpcImage.CreateAsync();

            ConnectionString = $"Server=host.docker.internal;Port={_postgres.GetMappedPublicPort(5432)};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";

            _migrationContainer = new ContainerBuilder()
                .WithImage(MIGRATION_IMAGE_NAME)
                .WithEnvironment("ConnectionStrings:Products", ConnectionString)
                .DependsOn(_postgres)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Migration completed successfully."))
                .Build();

            await _migrationContainer.StartAsync();

            GrpcContainer = new ContainerBuilder()
                .WithImage(GRPC_IMAGE_NAME)
                .WithPortBinding(8080, true)
                .WithExposedPort(8080)
                .WithEnvironment("ConnectionStrings:Products", ConnectionString)
                .Build();

            await GrpcContainer.StartAsync();

            var grpcConnectionString = $"http://{GrpcContainer.Hostname}:{GrpcContainer.GetMappedPublicPort(8080)}";
            Channel = GrpcChannel.ForAddress(grpcConnectionString);
        }

        public async Task DisposeAsync()
        {
            await _migrationContainer.DisposeAsync();
            await _postgres.DisposeAsync();
            await GrpcContainer.DisposeAsync();
            await Channel.ShutdownAsync();
        }
    }
}