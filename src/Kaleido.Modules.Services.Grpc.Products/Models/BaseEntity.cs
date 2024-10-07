using Kaleido.Modules.Services.Grpc.Products.Constants;

namespace Kaleido.Modules.Services.Grpc.Products.Models;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public string? Key { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int Revision { get; set; }
    public EntityStatus Status { get; set; }
}