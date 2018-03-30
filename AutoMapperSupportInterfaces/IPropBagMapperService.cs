using DRM.TypeSafePropertyBag;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropModelType = IPropModel<String>;

    public interface IPropBagMapperService : ICachePropBagMappers
    {
        // Typed Submit 
        IPropBagMapperKey<TSource, TDestination> SubmitPropBagMapperRequest<TSource, TDestination>
        (
            PropModelType propModel,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
        )
        where TDestination : class, IPropBag;

        // Gen Submit 
        IPropBagMapperKeyGen SubmitPropBagMapperRequest
        (
            PropModelType propModel,
            Type sourceType,
            string configPackageName
        );

        // The following two methods are provided by ICachePropBagMappers

        ////    Typed Get Mapper
        //      IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
        //      (
        //          IPropBagMapperKey<TSource, TDestination> mapperRequest
        //      )
        //      where TDestination : class, IPropBag;

        ////    Gen Get Mapper
        //      IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapperRequest);
    }
}
