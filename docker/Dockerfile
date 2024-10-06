# Use dotnet 8 image as base to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory in the container
WORKDIR /app

# Copy the source code and build the application
COPY ./src/Kaleido.Modules.Services.Grpc.Products ./

# Restore the dependencies
RUN dotnet restore

RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Kaleido.Modules.Services.Grpc.Products.dll"]