using Kaleido.Modules.Services.Grpc.Products.Common.Constants;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;

public class ValidationError(IEnumerable<string> path, ValidationErrorType errorType, string errorMessage)
{
    public ValidationErrorType Type { get; } = errorType;
    public string Error { get; } = errorMessage;
    public IEnumerable<string> Path { get; private set; } = path;

    public ValidationError PrependPath(IEnumerable<string> prefix)
    {
        Path = prefix.Concat(Path);
        return this;
    }

    public ValidationError RemovePathPrefix(IEnumerable<string> prefix)
    {
        Path = Path.SkipWhile(p => prefix.Contains(p));
        return this;
    }
}
