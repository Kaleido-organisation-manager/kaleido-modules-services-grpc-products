using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.GetAllByName;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAllByName;

public class GetAllByNameRequestValidatorTests
{
    private readonly GetAllByNameRequestValidator _validator;

    public GetAllByNameRequestValidatorTests()
    {
        _validator = new GetAllByNameRequestValidator();
    }

    [Fact]
    public async Task ValidateAsync_NameProvided_ReturnsValidResult()
    {
        // Arrange
        var request = new GetAllProductsByNameRequest();
        request.Name = "Test";

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_NameNotProvided_ReturnsInvalidResult()
    {
        // Arrange
        var request = new GetAllProductsByNameRequest();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }
}
