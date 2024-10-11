using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAllByCategoryKey;

public class GetAllByCategoryKeyIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetAllByCategoryKeyIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllByCategoryKey_ShouldReturnProducts()
    {
        var categoryKey = Guid.NewGuid().ToString();

        var createProducts = new List<CreateProduct>() {
            new CreateProductBuilder().WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithCategoryKey(categoryKey).Build(),
        };

        foreach (var product in createProducts)
        {
            await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
        }

        var response = await _fixture.Client.GetAllProductsByCategoryKeyAsync(new GetAllProductsByCategoryKeyRequest { CategoryKey = categoryKey });

        Assert.Equal(createProducts.Count, response.Products.Count);
    }

    [Fact]
    public async Task GetAllByCategoryKey_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        var response = await _fixture.Client.GetAllProductsByCategoryKeyAsync(new GetAllProductsByCategoryKeyRequest { CategoryKey = Guid.NewGuid().ToString() });

        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task GetAllByCategoryKey_ShouldReturnEmptyList_WhenNoProductsExistForCategory()
    {
        var categoryKey = Guid.NewGuid().ToString();

        var createProducts = new List<CreateProduct>() {
            new CreateProductBuilder().WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithCategoryKey(categoryKey).Build(),
        };

        foreach (var product in createProducts)
        {
            await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
        }

        var response = await _fixture.Client.GetAllProductsByCategoryKeyAsync(new GetAllProductsByCategoryKeyRequest { CategoryKey = Guid.NewGuid().ToString() });

        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task GetAllByCategoryKey_ShouldReturnMatchingProducts_WhenMultipleCategoriesExist()
    {
        var categoryKey = Guid.NewGuid().ToString();

        var createProducts = new List<CreateProduct>() {
            new CreateProductBuilder().WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithCategoryKey(categoryKey).Build(),
        };

        foreach (var product in createProducts)
        {
            await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
        }

        var categoryKeyToCheck = Guid.NewGuid().ToString();

        var createProductsWithCorrectCategoryKey = new List<CreateProduct>() {
            new CreateProductBuilder().WithCategoryKey(categoryKeyToCheck).Build(),
            new CreateProductBuilder().WithCategoryKey(categoryKeyToCheck).Build(),
        };

        foreach (var product in createProductsWithCorrectCategoryKey)
        {
            await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
        }

        var response = await _fixture.Client.GetAllProductsByCategoryKeyAsync(new GetAllProductsByCategoryKeyRequest { CategoryKey = categoryKey });

        Assert.Equal(createProductsWithCorrectCategoryKey.Count, response.Products.Count);
    }
}
