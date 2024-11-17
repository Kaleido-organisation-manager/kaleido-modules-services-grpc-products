using AutoMapper;
using FluentValidation;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Validators;
using Microsoft.Extensions.Logging;

namespace Kaleido.Modules.Services.Grpc.Products.GetAllFiltered;

public class GetAllFilteredHandler : IGetAllFilteredHandler
{
    private readonly IGetAllFilteredManager _manager;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFilteredHandler> _logger;
    private readonly KeyValidator _keyValidator;
    private readonly NameValidator _nameValidator;

    public GetAllFilteredHandler(
        IGetAllFilteredManager manager,
        IMapper mapper,
        ILogger<GetAllFilteredHandler> logger,
        KeyValidator keyValidator,
        NameValidator nameValidator)
    {
        _manager = manager;
        _mapper = mapper;
        _logger = logger;
        _keyValidator = keyValidator;
        _nameValidator = nameValidator;
    }

    public async Task<ProductListResponse> HandleAsync(
        ProductFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(request.CategoryKey))
            {
                _keyValidator.ValidateAndThrow(request.CategoryKey);
            }
            if (!string.IsNullOrEmpty(request.Name))
            {
                _nameValidator.ValidateAndThrow(request.Name);
            }

            var products = await _manager.GetAllFilteredAsync(
                request.Name,
                request.CategoryKey,
                cancellationToken);

            var response = new ProductListResponse();
            foreach (var item in products)
            {
                var productWithPricesResult = _mapper.Map<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>(item.Product);
                productWithPricesResult.Entity.Prices = item.ProductPrices ?? [];
                response.Products.Add(_mapper.Map<ProductResponse>(productWithPricesResult));
            }

            return response;
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "Validation error occurred while getting filtered products");
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting filtered products");
            throw new RpcException(new Status(StatusCode.Internal, "An internal error occurred"));
        }
    }
}