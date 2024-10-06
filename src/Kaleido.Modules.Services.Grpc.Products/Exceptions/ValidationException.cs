namespace Kaleido.Modules.Services.Grpc.Products.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}