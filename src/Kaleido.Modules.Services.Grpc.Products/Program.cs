using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Common.Handlers;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Repositories;
using Kaleido.Modules.Services.Grpc.Products.Common.Services;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Microsoft.EntityFrameworkCore;
using Kaleido.Modules.Services.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevision;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Kaleido.Modules.Services.Grpc.Products.Update;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Kaleido.Modules.Services.Grpc.Products.GetAll;
using Kaleido.Modules.Services.Grpc.Products.GetAllByCategoryKey;
using Kaleido.Modules.Services.Grpc.Products.GetProductRevisions;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevision;
using Kaleido.Modules.Services.Grpc.Products.GetProductPriceRevisions;
using Kaleido.Grpc.Products;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// Add Handlers
builder.Services.AddScoped<IBaseHandler<GetProductRevisionRequest, GetProductRevisionResponse>, GetProductRevisionHandler>();
builder.Services.AddScoped<IBaseHandler<CreateProductRequest, CreateProductResponse>, CreateHandler>();
builder.Services.AddScoped<IBaseHandler<UpdateProductRequest, UpdateProductResponse>, UpdateHandler>();
builder.Services.AddScoped<IBaseHandler<DeleteProductRequest, DeleteProductResponse>, DeleteHandler>();
builder.Services.AddScoped<IBaseHandler<GetProductRequest, GetProductResponse>, GetHandler>();
builder.Services.AddScoped<IBaseHandler<GetAllProductsRequest, GetAllProductsResponse>, GetAllHandler>();
builder.Services.AddScoped<IBaseHandler<GetAllProductsByCategoryKeyRequest, GetAllProductsByCategoryKeyResponse>, GetAllByCategoryKeyHandler>();
builder.Services.AddScoped<IBaseHandler<GetProductRevisionsRequest, GetProductRevisionsResponse>, GetProductRevisionsHandler>();
builder.Services.AddScoped<IBaseHandler<GetProductPriceRevisionRequest, GetProductPriceRevisionResponse>, GetProductPriceRevisionHandler>();
builder.Services.AddScoped<IBaseHandler<GetProductPriceRevisionsRequest, GetProductPriceRevisionsResponse>, GetProductPriceRevisionsHandler>();

// Add Managers
builder.Services.AddScoped<IGetManager, GetManager>();
builder.Services.AddScoped<ICreateManager, CreateManager>();
builder.Services.AddScoped<IUpdateManager, UpdateManager>();
builder.Services.AddScoped<IDeleteManager, DeleteManager>();
builder.Services.AddScoped<IGetAllManager, GetAllManager>();
builder.Services.AddScoped<IGetAllByCategoryKeyManager, GetAllByCategoryKeyManager>();
builder.Services.AddScoped<IGetProductRevisionManager, GetProductRevisionManager>();
builder.Services.AddScoped<IGetProductRevisionsManager, GetProductRevisionsManager>();
builder.Services.AddScoped<IGetProductPriceRevisionManager, GetProductPriceRevisionManager>();
builder.Services.AddScoped<IGetProductPriceRevisionsManager, GetProductPriceRevisionsManager>();

// Add Mappers
builder.Services.AddScoped<IProductMapper, ProductMapper>();

// Add Repositories
builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
builder.Services.AddScoped<IProductPricesRepository, ProductPricesRepository>();

// Add Validators
builder.Services.AddScoped<IProductValidator, ProductValidator>();
builder.Services.AddScoped<IProductPriceValidator, ProductPriceValidator>();
builder.Services.AddScoped<IRequestValidator<CreateProductRequest>, CreateRequestValidator>();
builder.Services.AddScoped<IRequestValidator<UpdateProductRequest>, UpdateRequestValidator>();
builder.Services.AddScoped<IRequestValidator<DeleteProductRequest>, DeleteRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetProductRequest>, GetRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetAllProductsRequest>, GetAllRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetAllProductsByCategoryKeyRequest>, GetAllByCategoryKeyRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetProductRevisionRequest>, GetProductRevisionRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetProductRevisionsRequest>, GetProductRevisionsRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetProductPriceRevisionRequest>, GetProductPriceRevisionRequestValidator>();
builder.Services.AddScoped<IRequestValidator<GetProductPriceRevisionsRequest>, GetProductPriceRevisionsRequestValidator>();


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