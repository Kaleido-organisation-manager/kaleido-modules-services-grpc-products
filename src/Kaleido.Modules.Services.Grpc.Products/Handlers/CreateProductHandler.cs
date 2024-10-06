using Grpc.Core;
using Kaleido.Modules.Services.Grpc.Products.Exceptions;
using Kaleido.Modules.Services.Grpc.Products.Handlers.Interfaces;
using Kaleido.Modules.Services.Grpc.Products.Managers.Interfaces;

namespace Kaleido.Modules.Services.Grpc.Products.Handlers
{
    public class CreateProductHandler : ICreateProductHandler
    {
        private readonly ILogger<CreateProductHandler> _logger;
        private readonly IProductsManager _productsManager;

        public CreateProductHandler(
            ILogger<CreateProductHandler> logger,
            IProductsManager productsManager
            )
        {
            _logger = logger;
            _productsManager = productsManager;
        }

        public async Task<CreateProductResponse> HandleAsync(CreateProduct createProduct, CancellationToken cancellationToken = default)
        {
            try
            {

                var product = await _productsManager.CreateProductAsync(createProduct, cancellationToken);

                return new CreateProductResponse
                {
                    Product = product
                };
            }
            catch (ValidationException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (Exception ex)
            {
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
        }
    }
}