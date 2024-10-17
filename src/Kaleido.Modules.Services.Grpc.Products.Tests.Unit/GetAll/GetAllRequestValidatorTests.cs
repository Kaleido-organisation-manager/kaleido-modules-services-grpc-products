using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;
using Kaleido.Modules.Services.Grpc.Products.GetAll;
using Moq;

namespace Kaleido.Modules.Services.Grpc.Products.Tests.Unit.GetAll;

public class GetAllRequestValidatorTests
{
    private readonly GetAllRequestValidator _validator;

    public GetAllRequestValidatorTests()
    {
        _validator = new GetAllRequestValidator();
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ReturnsValidResult()
    {
        // Arrange
        var request = new GetAllProductsRequest();

        // Act
        var result = await _validator.ValidateAsync(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
