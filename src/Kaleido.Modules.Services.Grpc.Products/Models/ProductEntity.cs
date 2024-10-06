namespace Kaleido.Modules.Services.Grpc.Products.Models;

public class ProductEntity : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string CategoryKey { get; set; }
}