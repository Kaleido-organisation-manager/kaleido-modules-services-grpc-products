using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Exceptions;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public IEnumerable<ValidationError> Errors { get; private set; } = [];

    public ValidationResult AddError(IEnumerable<string> path, ValidationErrorType errorType, string errorMessage)
    {
        Errors = Errors.Append(new ValidationError(path, errorType, errorMessage));
        return this;
    }

    public ValidationResult AddNotFoundError(IEnumerable<string> path, string errorMessage)
    {
        return AddError(path, ValidationErrorType.NotFound, errorMessage);
    }

    public ValidationResult AddAlreadyExistsError(IEnumerable<string> path, string errorMessage)
    {
        return AddError(path, ValidationErrorType.AlreadyExists, errorMessage);
    }

    public ValidationResult AddInvalidFormatError(IEnumerable<string> path, string errorMessage)
    {
        return AddError(path, ValidationErrorType.InvalidFormat, errorMessage);
    }

    public ValidationResult AddRequiredError(IEnumerable<string> path, string errorMessage)
    {
        return AddError(path, ValidationErrorType.Required, errorMessage);
    }

    public ValidationResult AddDataConflictError(IEnumerable<string> path, string errorMessage)
    {
        return AddError(path, ValidationErrorType.DataConflict, errorMessage);
    }

    public ValidationResult Merge(ValidationResult validationResult)
    {
        Errors = Errors.Concat(validationResult.Errors);
        return this;
    }

    public ValidationResult PrependPath(IEnumerable<string> prefix)
    {
        return new ValidationResult
        {
            Errors = Errors.Select(e => e.PrependPath(prefix))
        };
    }

    public ValidationResult RemovePathPrefix(IEnumerable<string> prefix)
    {
        return new ValidationResult
        {
            Errors = Errors.Select(e => e.RemovePathPrefix(prefix))
        };
    }

    public void ThrowIfInvalid()
    {
        if (!IsValid)
        {
            throw new ValidationException("Validation failed", Errors);
        }
    }
}
