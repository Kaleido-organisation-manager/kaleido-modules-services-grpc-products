namespace Kaleido.Modules.Services.Grpc.Products.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
    }
}
