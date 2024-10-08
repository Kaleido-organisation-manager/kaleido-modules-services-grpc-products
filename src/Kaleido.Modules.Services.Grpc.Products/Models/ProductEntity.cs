namespace Kaleido.Modules.Services.Grpc.Products.Models;

public class ProductEntity : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required Guid CategoryKey { get; set; }
    public string? ImageUrl { get; set; }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj) &&
            obj is ProductEntity entity &&
            Name == entity.Name &&
            CategoryKey == entity.CategoryKey &&
            Description == entity.Description &&
            ImageUrl == entity.ImageUrl;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Name, CategoryKey, Description, ImageUrl);
    }
}