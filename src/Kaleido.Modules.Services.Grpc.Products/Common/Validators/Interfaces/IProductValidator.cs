using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models.Validations;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

public interface IProductValidator
{
    Task<ValidationResult> ValidateCreateAsync(CreateProduct product, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateUpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateKeyAsync(string productKey, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateCategoryKeyAsync(string categoryKey, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateKeyForRevisionAsync(string productKey, CancellationToken cancellationToken = default);
    ValidationResult ValidateKeyFormat(string productKey);
}
