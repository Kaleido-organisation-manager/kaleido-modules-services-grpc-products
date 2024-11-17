using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Handlers.Extensions;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Kaleido.Modules.Services.Grpc.Categories.Client.Extensions;
using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;
using Kaleido.Modules.Services.Grpc.Products.Common.Mappers;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Services;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Kaleido.Modules.Services.Grpc.Products.Create;
using Kaleido.Modules.Services.Grpc.Products.Delete;
using Kaleido.Modules.Services.Grpc.Products.Get;
using Kaleido.Modules.Services.Grpc.Products.GetAll;

var builder = WebApplication.CreateBuilder(args);

//Common
var Configuration = builder.Configuration;
var productsConnectionString = Configuration.GetConnectionString("Products");
if (string.IsNullOrEmpty(productsConnectionString))
{
    throw new ArgumentNullException(nameof(productsConnectionString), "No connection string found to connect to the products database");
}

var categoriesConnectionString = Configuration.GetConnectionString("Categories");
if (string.IsNullOrEmpty(categoriesConnectionString))
{
    throw new ArgumentNullException(nameof(categoriesConnectionString), "No connection string found to connect to the categories database");
}

builder.Services.AddCategoryClient(categoriesConnectionString);

builder.Services.AddAutoMapper(typeof(ProductMappingProfile));
builder.Services.AddScoped<CategoryKeyValidator>();
builder.Services.AddScoped<CurrencyKeyValidator>();
builder.Services.AddScoped<KeyValidator>();
builder.Services.AddScoped<NameValidator>();
builder.Services.AddScoped<ProductPriceValidator>();
builder.Services.AddScoped<ProductValidator>();

builder.Services.AddKaleidoEntityDbContext<ProductEntity, ProductEntityDbContext>(productsConnectionString);
builder.Services.AddKaleidoRevisionDbContext<ProductRevisionEntity, ProductRevisionDbContext>(productsConnectionString);
builder.Services.AddKaleidoEntityDbContext<ProductPriceEntity, ProductPriceEntityDbContext>(productsConnectionString);
builder.Services.AddKaleidoRevisionDbContext<ProductPriceRevisionEntity, ProductPriceRevisionDbContext>(productsConnectionString);

builder.Services.AddEntityRepository<ProductEntity, ProductEntityDbContext>();
builder.Services.AddRevisionRepository<ProductRevisionEntity, ProductRevisionDbContext>();
builder.Services.AddEntityRepository<ProductPriceEntity, ProductPriceEntityDbContext>();
builder.Services.AddRevisionRepository<ProductPriceRevisionEntity, ProductPriceRevisionDbContext>();

builder.Services.AddLifeCycleHandler<ProductEntity, ProductRevisionEntity>();
builder.Services.AddLifeCycleHandler<ProductPriceEntity, ProductPriceRevisionEntity>();

// Create
builder.Services.AddScoped<ICreateManager, CreateManager>();
builder.Services.AddScoped<ICreateHandler, CreateHandler>();

// Delete
builder.Services.AddScoped<IDeleteManager, DeleteManager>();
builder.Services.AddScoped<IDeleteHandler, DeleteHandler>();

// Get
builder.Services.AddScoped<IGetHandler, GetHandler>();
builder.Services.AddScoped<IGetManager, GetManager>();

// GetAll
builder.Services.AddScoped<IGetAllHandler, GetAllHandler>();
builder.Services.AddScoped<IGetAllManager, GetAllManager>();

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProductsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }