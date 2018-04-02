using System;
using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperService : IAutoMapperCache
    {
        //Typed Submit with ConfigPackageName
        IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
        (
            IMapTypeDefinition srcMapTypeDef,
            IMapTypeDefinition dstMapTypeDef,
            IAutoMapperConfigDetails configuationDetails,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest
        );

        //Typed Submit with a Mapping Configuration (IConfigureAMapper)
        IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
        (
            IMapTypeDefinition srcMapTypeDef,
            IMapTypeDefinition dstMapTypeDef,
            IAutoMapperConfigDetails configuationDetails,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IHaveAMapperConfigurationStep configStarterForThisRequest
        );

        // Typed Get Mapper
        IMapper GetRawAutoMapper<TSource, TDestination>
        (
            IAutoMapperRequestKey<TSource, TDestination> mapperRequest
        );

        // Gen Submit 
        IAutoMapperRequestKeyGen SubmitRawAutoMapperRequest
        (
            Type sourceType,
            Type destType,
            IAutoMapperConfigDetails configuationDetails,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest
        );

        // Provided by ICacheAutoMappers
        //IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
    }

}