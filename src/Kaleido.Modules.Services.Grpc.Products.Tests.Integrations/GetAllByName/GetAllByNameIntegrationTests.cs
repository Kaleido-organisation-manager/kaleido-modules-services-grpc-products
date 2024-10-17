using Grpc.Core;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Builders;
using Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.Fixtures;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Integrations.GetAllByName;

public class GetAllByNameIntegrationTests : IClassFixture<InfrastructureFixture>
{
    private readonly InfrastructureFixture _fixture;

    public GetAllByNameIntegrationTests(InfrastructureFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllByName_ShouldReturnMatchingProducts()
    {
        // Arrange
        var productName = "TestProduct";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName(productName).Build(),
            new CreateProductBuilder().WithName(productName + "2").Build(),
            new CreateProductBuilder().WithName("DifferentProduct").Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAsync(new GetAllProductsByNameRequest { Name = productName });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.All(response.Products, p => Assert.Contains(productName, p.Name));

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }

    [Fact]
    public async Task GetAllByName_ShouldReturnEmptyList_WhenNoProductsMatch()
    {
        // Arrange
        var nonExistentName = "NonExistentProduct";

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAsync(new GetAllProductsByNameRequest { Name = nonExistentName });

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Products);
    }

    [Fact]
    public async Task GetAllByName_ShouldReturnValidationException_WhenNameIsEmpty()
    {
        // Arrange
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().Build(),
            new CreateProductBuilder().Build(),
            new CreateProductBuilder().Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act && Assert
        var exception = await Assert.ThrowsAsync<RpcException>(async () => await _fixture.Client.GetAllProductsByNameAsync(new GetAllProductsByNameRequest { Name = "" }));
        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }

    [Fact]
    public async Task GetAllByName_ShouldBeCaseInsensitive()
    {
        // Arrange
        var productName = "TestProduct";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName(productName.ToLower()).Build(),
            new CreateProductBuilder().WithName(productName.ToUpper()).Build(),
            new CreateProductBuilder().WithName("DifferentProduct").Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAsync(new GetAllProductsByNameRequest { Name = productName });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Products.Count);
        Assert.All(response.Products, p => Assert.Contains(productName, p.Name, StringComparison.OrdinalIgnoreCase));

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }

    [Fact]
    public async Task GetAllByName_ShouldReturnPartialMatches()
    {
        // Arrange
        var productNamePart = "Test";
        var createProducts = new List<CreateProduct>
        {
            new CreateProductBuilder().WithName($"{productNamePart}Product1").Build(),
            new CreateProductBuilder().WithName($"Product2{productNamePart}").Build(),
            new CreateProductBuilder().WithName($"{productNamePart}Product3{productNamePart}").Build(),
            new CreateProductBuilder().WithName("DifferentProduct").Build()
        };

        var createdProducts = new List<Product>();

        foreach (var product in createProducts)
        {
            var createdProduct = await _fixture.Client.CreateProductAsync(new CreateProductRequest { Product = product });
            createdProducts.Add(createdProduct.Product);
        }

        // Act
        var response = await _fixture.Client.GetAllProductsByNameAsync(new GetAllProductsByNameRequest { Name = productNamePart });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Products.Count);
        Assert.All(response.Products, p => Assert.Contains(productNamePart, p.Name, StringComparison.OrdinalIgnoreCase));

        foreach (var product in createdProducts)
        {
            await _fixture.Client.DeleteProductAsync(new DeleteProductRequest { Key = product.Key });
        }
    }
}