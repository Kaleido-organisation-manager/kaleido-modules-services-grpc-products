using Kaleido.Modules.Services.Grpc.Products.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers;
using Kaleido.Modules.Services.Grpc.Products.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Repositories;
using Kaleido.Modules.Services.Grpc.Products.Services;
using Kaleido.Modules.Services.Grpc.Products.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Validators;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Add Handlers
builder.Services.AddScoped<IGetProductHandler, GetProductHandler>();
builder.Services.AddScoped<IGetAllProductsHandler, GetAllProductsHandler>();
builder.Services.AddScoped<IGetAllProductsByCategoryIdHandler, GetAllProductsByCategoryIdHandler>();
builder.Services.AddScoped<ICreateProductHandler, CreateProductHandler>();

// Add Managers
builder.Services.AddScoped<IProductsManager, ProductsManager>();

// Add Repositories
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IProductPricesRepository, ProductPricesRepository>();

// Add Validators
builder.Services.AddScoped<ICategoryValidator, CategoryValidator>();
builder.Services.AddScoped<IProductValidator, ProductValidator>();
builder.Services.AddScoped<IProductPriceValidator, ProductPriceValidator>();


var Configuration = builder.Configuration;
builder.Services.AddDbContext<ProductsDbContext>(options =>
        options.UseNpgsql(Configuration.GetConnectionString("Products")));

builder.Services.AddDbContext<ProductPricesDbContext>(options =>
        options.UseNpgsql(Configuration.GetConnectionString("ProductPrices")));

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapHealthChecks("/health");

app.Run();
