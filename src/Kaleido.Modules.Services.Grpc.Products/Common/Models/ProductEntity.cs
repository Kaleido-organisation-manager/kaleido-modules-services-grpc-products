using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models;

public class ProductEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryKey { get; set; }
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