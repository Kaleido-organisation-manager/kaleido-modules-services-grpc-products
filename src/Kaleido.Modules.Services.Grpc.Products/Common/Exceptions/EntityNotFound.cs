namespace Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}
