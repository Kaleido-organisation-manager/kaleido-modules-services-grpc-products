using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

public class ValidationException : RpcException
{
    public IEnumerable<ValidationError> Errors { get; }
    public ValidationException(string message, IEnumerable<ValidationError> errors) : base(new Status(StatusCode.InvalidArgument, message))
    {
        Errors = errors;
    }
}