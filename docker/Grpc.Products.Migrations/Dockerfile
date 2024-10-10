# Use dotnet 8 image as base to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory in the container
WORKDIR /app

# Copy the source code and build the application
COPY ./src/Kaleido.Modules.Services.Grpc.Products.Migrations ./Kaleido.Modules.Services.Grpc.Products.Migrations
COPY ./src/Kaleido.Modules.Services.Grpc.Products ./Kaleido.Modules.Services.Grpc.Products
COPY ./src/Proto ./Proto

# Restore the dependencies
RUN dotnet restore Kaleido.Modules.Services.Grpc.Products.Migrations
RUN dotnet restore Kaleido.Modules.Services.Grpc.Products

RUN dotnet build Kaleido.Modules.Services.Grpc.Products.Migrations
RUN dotnet tool install --global dotnet-ef
RUN ls -la
RUN ls /root/.dotnet/tools

ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations add Products --project Kaleido.Modules.Services.Grpc.Products.Migrations

RUN dotnet publish Kaleido.Modules.Services.Grpc.Products.Migrations -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

ENV "Kestrel:EndpointDefaults:Protocols"="Http2"

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Kaleido.Modules.Services.Grpc.Products.Migrations.dll"]