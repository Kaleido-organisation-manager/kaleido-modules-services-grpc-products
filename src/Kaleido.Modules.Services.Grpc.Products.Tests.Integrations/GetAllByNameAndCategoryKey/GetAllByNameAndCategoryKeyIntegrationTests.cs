using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAllByNameAndCategoryKey;

public class GetAllByNameAndCategoryKeyIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetAllByNameAndCategoryKeyIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldReturnMatchingProducts()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();
        var productName = "TestProduct";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName(productName).WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName(productName + "2").WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName("DifferentProduct").WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName(productName).WithCategoryKey(Guid.NewGuid().ToString()).Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = productName, CategoryKey = categoryKey });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.All(response.Products, p => Assert.Contains(productName, p.Name));
        Assert.All(response.Products, p => Assert.Equal(categoryKey, p.CategoryKey));

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldReturnEmptyList_WhenNoProductsMatch()
    {
        // Arrange
        var nonExistentName = "NonExistentProduct";
        var nonExistentCategoryKey = Guid.NewGuid().ToString();

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = nonExistentName, CategoryKey = nonExistentCategoryKey });

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldReturnValidationException_WhenNameIsEmpty()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = "", CategoryKey = categoryKey }));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldReturnValidationException_WhenCategoryKeyIsInvalid()
    {
        // Arrange
        var productName = "TestProduct";
        var invalidCategoryKey = "invalid-guid";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () =>
            await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = productName, CategoryKey = invalidCategoryKey }));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldBeCaseInsensitiveForName()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();
        var productName = "TestProduct";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName(productName.ToLower()).WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName(productName.ToUpper()).WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName("DifferentProduct").WithCategoryKey(categoryKey).Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = productName, CategoryKey = categoryKey });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.All(response.Products, p => Assert.Contains(productName, p.Name, StringComparison.OrdinalIgnoreCase));
        Assert.All(response.Products, p => Assert.Equal(categoryKey, p.CategoryKey));

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldReturnPartialMatches()
    {
        // Arrange
        var categoryKey = Guid.NewGuid().ToString();
        var productNamePart = "Test";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName($"{productNamePart}Product1").WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName($"Product2{productNamePart}").WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName($"{productNamePart}Product3{productNamePart}").WithCategoryKey(categoryKey).Build(),
            new CreateProductBuilder().WithName("DifferentProduct").WithCategoryKey(categoryKey).Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = productNamePart, CategoryKey = categoryKey });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Products.Count);
        Assert.All(response.Products, p => Assert.Contains(productNamePart, p.Name, StringComparison.OrdinalIgnoreCase));
        Assert.All(response.Products, p => Assert.Equal(categoryKey, p.CategoryKey));

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }

    [Fact]
    public async Task GetAllByNameAndCategoryKey_ShouldNotReturnProductsFromOtherCategories()
    {
        // Arrange
        var categoryKey1 = Guid.NewGuid().ToString();
        var categoryKey2 = Guid.NewGuid().ToString();
        var productName = "TestProduct";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName(productName).WithCategoryKey(categoryKey1).Build(),
            new CreateProductBuilder().WithName(productName).WithCategoryKey(categoryKey2).Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAndCategoryKeyAsync(new GetAllProductsByNameAndCategoryKeyRequest { Name = productName, CategoryKey = categoryKey1 });

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Products);

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }
}

