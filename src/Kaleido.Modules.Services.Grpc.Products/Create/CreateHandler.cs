using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Create;

public class CreateHandler : ICreateHandler
{
    private readonly ICreateManager _createManager;
    private readonly ILogger<CreateHandler> _logger;
    private readonly IMapper _mapper;
    private readonly ProductValidator _productValidator;

    public CreateHandler(
        ICreateManager createManager,
        ILogger<CreateHandler> logger,
        IMapper mapper,
        ProductValidator productValidator)
    {
        _createManager = createManager;
        _logger = logger;
        _mapper = mapper;
        _productValidator = productValidator;
    }

    public async Task<ProductResponse> HandleAsync(Product request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _productValidator.ValidateAndThrowAsync(request, cancellationToken);
            var product = _mapper.Map<ProductEntity>(request);

            var managerResponse = await _createManager.CreateAsync(product, request.Prices, cancellationToken);

            var productResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(managerResponse.Product);
            productResult.Entity.Prices = managerResponse.ProductPrices ?? [];

            var response = _mapper.Map<ProductResponse>(productResult);
            return response;
        }
        catch (ValidationException e)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message, e));
        }
        catch (Exception e)
        {
            throw new RpcException(new Status(StatusCode.Internal, e.Message, e));
        }
    }
}