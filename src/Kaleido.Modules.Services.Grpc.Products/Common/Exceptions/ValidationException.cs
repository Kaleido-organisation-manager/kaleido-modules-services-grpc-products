namespace Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }
}