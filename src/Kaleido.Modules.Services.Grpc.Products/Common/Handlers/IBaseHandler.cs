using Kaleido.Modules.Services.Grpc.Products.Common.Validators.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Handlers;

public interface IBaseHandler<TRequest, TResponse>
{
    public IRequestValidator<TRequest> Validator { get; }
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}