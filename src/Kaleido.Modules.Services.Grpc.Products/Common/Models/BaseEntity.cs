using Kaleido.Modules.Services.Grpc.Products.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid Key { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Revision { get; set; }
    public EntityStatus Status { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var entity = (BaseEntity)obj;
        return Key == entity.Key;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key);
    }
}