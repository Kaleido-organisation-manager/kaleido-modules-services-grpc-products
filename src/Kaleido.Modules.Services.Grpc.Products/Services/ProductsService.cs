using Grpc.Core;

namespace Kaleido.Modules.Services.Grpc.Products.Services;

public class ProductsService : Products.ProductsBase
{
    private readonly ILogger<ProductsService> _logger;
    public ProductsService(ILogger<ProductsService> logger)
    {
        _logger = logger;
    }

    public override Task<GetProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        _logger.LogInformation("GetProduct called with Id: {Id}", request.Id);
        return Task.FromResult(new GetProductResponse
        {
            Product = new Product
            {
                Id = request.Id,
                Name = "Product 1",
                Description = "Description of Product 1",
                Price = 100
            }
        });
    }

}