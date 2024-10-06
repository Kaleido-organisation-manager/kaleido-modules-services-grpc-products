using Kaleido.Modules.Services.Grpc.Products.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapHealthChecks("/health");

app.Run();
