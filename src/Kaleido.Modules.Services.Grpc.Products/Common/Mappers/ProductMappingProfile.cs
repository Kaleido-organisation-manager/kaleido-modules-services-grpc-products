using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Grpc.Products;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;

namespace Kaleido.Modules.Services.Grpc.Products.Common.Mappers;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Basic entity mappings
        CreateMap<Product, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForSourceMember(src => src.Prices, opt => opt.DoNotValidate());
        CreateMap<ProductEntity, ProductWithPrices>()
            .ForMember(dest => dest.Prices, opt => opt.Ignore()); // Or map from appropriate source
        CreateMap<ProductPrice, ProductPriceEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductKey, opt => opt.Ignore());

        // Response mappings
        CreateMap<EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>, ProductResponse>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));
        CreateMap<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>, ProductPriceResponse>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));

        CreateMap<ProductPriceEntity, ProductPrice>();
        CreateMap<ProductWithPrices, ProductWithPricesResponse>();

        // Revision mappings
        CreateMap<BaseRevisionEntity, BaseRevision>();
        CreateMap<ProductPriceRevisionEntity, BaseRevision>();
        CreateMap<ProductRevisionEntity, BaseRevision>();

        // DateTime <-> Timestamp conversions
        CreateMap<Timestamp, DateTime>().ConvertUsing(src => src.ToDateTime());
        CreateMap<DateTime, Timestamp>().ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));

        // Mapping for composite types
        CreateMap<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>, EntityLifeCycleResult<ProductWithPrices, BaseRevisionEntity>>()
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));

        // Self mappings for entity lifecycle results
        CreateMap<EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>, EntityLifeCycleResult<ProductPriceEntity, ProductPriceRevisionEntity>>()
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));
        CreateMap<ProductPriceEntity, ProductPriceEntity>();
        CreateMap<ProductPriceRevisionEntity, ProductPriceRevisionEntity>();

        CreateMap<EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>, EntityLifeCycleResult<ProductEntity, ProductRevisionEntity>>()
            .ForMember(dest => dest.Entity, opt => opt.MapFrom(src => src.Entity))
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision));
        CreateMap<ProductRevisionEntity, ProductRevisionEntity>();
        CreateMap<ProductEntity, ProductEntity>();
    }
}
