# Kaleido Products gRPC Service

This repository contains the gRPC service for managing products in the Kaleido Organisation Manager system.

## Projects in the Solution

The solution consists of the following projects:

1. **Kaleido.Modules.Services.Grpc.Products**: The main gRPC service project.
2. **Kaleido.Modules.Services.Grpc.Products.Client**: A client library for consuming the gRPC service.
3. **Kaleido.Modules.Services.Grpc.Products.Migrations**: A project for managing database migrations.
4. **Kaleido.Modules.Services.Grpc.Products.Tests.Unit**: Unit tests for the service.
5. **Kaleido.Modules.Services.Grpc.Products.Tests.Integrations**: Integration tests for the service.

## Building and Running

### Prerequisites

- .NET 8.0 SDK
- Docker (for running integration tests and containerized deployments)
- PostgreSQL (for local development)

### Building the Solution

To build the entire solution, run the following command from the root directory:

```bash
dotnet build
```


### Running the gRPC Service

To run the gRPC service locally:

1. Ensure you have a PostgreSQL instance running and accessible.
2. Set the required environment variables (see Environment Variables section).
3. Navigate to the `src/Kaleido.Modules.Services.Grpc.Products` directory.
4. Run the following command:

```bash
dotnet run
```

### Running Tests

To run unit tests:

```bash
dotnet test src/Kaleido.Modules.Services.Grpc.Products.Tests.Unit
```

To run integration tests:

```bash
dotnet test src/Kaleido.Modules.Services.Grpc.Products.Tests.Integrations
```


Note: Integration tests require Docker to be running on your machine.

## Environment Variables

The following environment variables are required for running the service and tests:

- `ConnectionStrings:Products`: The connection string for the PostgreSQL database.

Example:

```bash
ConnectionStrings:Products="Server=localhost;Port=5432;Database=kaleido_products;User Id=postgres;Password=mysecretpassword;
```


For integration tests, you may need to set:

- `PRODUCTS_IMAGE_NAME`: The Docker image name for the products service.
- `MIGRATIONS_IMAGE_NAME`: The Docker image name for the migrations service.

These are typically set automatically in the CI/CD pipeline.

## Docker

The repository includes Dockerfiles for both the gRPC service and the migrations project. You can build the Docker images using the following commands:

For the gRPC service:

```bash
docker build -t kaleido-products-service -f src/Kaleido.Modules.Services.Grpc.Products/Dockerfile .
```

For the migrations project:

```bash
docker build -t kaleido-products-migrations -f src/Kaleido.Modules.Services.Grpc.Products.Migrations/Dockerfile .
```


## CI/CD

The repository includes GitHub Actions workflows for CI/CD:

- `github-actions.yaml`: Runs on merges to the main branch, performing builds, tests, and deployments.
- `github-actions-pr.yaml`: Runs on pull requests, performing builds and tests.

## Proto Files

The gRPC service definitions can be found in the `src/Proto` directory. These files define the service interface and message types used in the gRPC communication.

## Contributing

Please refer to the repository's contributing guidelines for information on how to contribute to this project.
