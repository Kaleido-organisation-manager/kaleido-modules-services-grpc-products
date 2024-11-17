using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Get;

public class GetHandler : IGetHandler
{
    private readonly IMapper _mapper;
    private readonly KeyValidator _validator;
    private readonly IGetManager _getManager;

    public GetHandler(
        IMapper mapper,
        KeyValidator validator,
        IGetManager getManager)
    {
        _mapper = mapper;
        _validator = validator;
        _getManager = getManager;
    }

    public async Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? managerResult;
        try
        {
            await _validator.ValidateAndThrowAsync(request.Key, cancellationToken);

            managerResult = await _getManager.GetAsync(Guid.Parse(request.Key), cancellationToken);
        }
        catch (ValidationException e)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, e.Message, e));
        }
        catch (Exception e)
        {
            throw new RpcException(new Status(StatusCode.Internal, e.Message, e));
        }


        if (managerResult == null || managerResult.Value.State != ManagerResponseState.Success)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with key {request.Key} not found"));
        }

        var productWithPricesResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(managerResult.Value.Product);
        productWithPricesResult.Entity.Prices = managerResult.Value.ProductPrices ?? [];

        return _mapper.Map<ProductResponse>(productWithPricesResult);
    }
}