using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Constants;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;

namespace Kaleido.Modules.Services.Grpc.Products.Delete;

public class DeleteHandler : IDeleteHandler
{
    private readonly IDeleteManager _deleteManager;
    private readonly ILogger<DeleteHandler> _logger;
    private readonly IMapper _mapper;
    private readonly KeyValidator _keyValidator;

    public DeleteHandler(
        IDeleteManager deleteManager,
        ILogger<DeleteHandler> logger,
        IMapper mapper,
        KeyValidator keyValidator)
    {
        _deleteManager = deleteManager;
        _logger = logger;
        _mapper = mapper;
        _keyValidator = keyValidator;
    }

    public async Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken = default)
    {
        ManagerResponse? managerResult;
        try
        {
            _keyValidator.ValidateAndThrow(request.Key);
            var key = Guid.Parse(request.Key);
            managerResult = await _deleteManager.DeleteAsync(key, cancellationToken);
        }
        catch (FormatException e)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid key format", e));
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

        var productResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(managerResult.Value.Product);
        productResult.Entity.Prices = managerResult.Value.ProductPrices ?? [];

        var response = _mapper.Map<ProductResponse>(productResult);
        return response;
    }
}