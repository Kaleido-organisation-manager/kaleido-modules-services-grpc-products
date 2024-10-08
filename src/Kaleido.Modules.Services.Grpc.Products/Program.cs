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
using Kaleido.Modules.Services.Grpc.Products.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Mappers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Add Handlers
builder.Services.AddScoped<ICreateProductHandler, CreateProductHandler>();
builder.Services.AddScoped<IDeleteProductHandler, DeleteProductHandler>();
builder.Services.AddScoped<IGetAllProductsByCategoryIdHandler, GetAllProductsByCategoryIdHandler>();
builder.Services.AddScoped<IGetAllProductsHandler, GetAllProductsHandler>();
builder.Services.AddScoped<IGetProductHandler, GetProductHandler>();
builder.Services.AddScoped<IGetProductRevisionHandler, GetProductRevisionHandler>();
builder.Services.AddScoped<IGetProductRevisionsHandler, GetProductRevisionsHandler>();
builder.Services.AddScoped<IUpdateProductHandler, UpdateProductHandler>();

// Add Managers
builder.Services.AddScoped<IProductsManager, ProductsManager>();

// Add Mappers
builder.Services.AddScoped<IProductMapper, ProductMapper>();

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

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
        var dbContext = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();
        dbContext.Database.Migrate();  // Applies any pending migrations
}

app.Run();

public partial class Program { }