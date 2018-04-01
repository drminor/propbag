using System;
using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    //using PropModelType = IPropModel<String>;

    public interface IAutoMapperService : IAutoMapperCache
    {
        //Typed Submit with ConfigPackageName
        IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
        (
            //PropModelType propModel,
            IMapTypeDefinition srcMapTypeDef,
            IMapTypeDefinition dstMapTypeDef,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest
        );
        //where TDestination : class, IPropBag;

        //Typed Submit with a Mapping Configuration (IConfigureAMapper)
        IAutoMapperRequestKey<TSource, TDestination> SubmitRawAutoMapperRequest<TSource, TDestination>
        (
            //PropModelType propModel,
            IMapTypeDefinition srcMapTypeDef,
            IMapTypeDefinition dstMapTypeDef,
            IConfigureAMapper<TSource, TDestination> mappingConfiguration,
            IHaveAMapperConfigurationStep configStarterForThisRequest
        );
        //where TDestination : class, IPropBag;

        // Typed Get Mapper
        IMapper GetRawAutoMapper<TSource, TDestination>
        (
            IAutoMapperRequestKey<TSource, TDestination> mapperRequest
        );
        //where TDestination : class, IPropBag;

        // Gen Submit 
        IAutoMapperRequestKeyGen SubmitRawAutoMapperRequest
        (
            //PropModelType propModel,
            Type sourceType,
            Type destType,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest
        );

        // Provided by ICacheAutoMappers
        //IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
    }

}