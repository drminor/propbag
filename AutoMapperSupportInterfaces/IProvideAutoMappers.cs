using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    // TODO: Change Register<xxx> to Submit<xxx>. (We are submitting a notice that we will need this at some point -- remember: GetMapper creates a mapper for all pending requests on first access to any request.)
    public interface IProvideAutoMappers : ICachePropBagMappers
    {
        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>
            (
            PropModelType propModel,
            Type typeToWrap,
            string configPackageName,
            IHaveAMapperConfigurationStep configStarterForThisRequest = null,
            IPropFactory propFactory = null
            )
            where TDestination : class, IPropBag;

        // The PropModel supplies the TDestination type and the Type that will be wrapped.
        IPropBagMapperKeyGen RegisterMapperRequest
            (
            PropModelType propModel,
            Type sourceType,
            string configPackageName
            );


        // Execute the Request and get the Mapper
        IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>
            (
            IPropBagMapperKey<TSource, TDestination> mapperRequest
            )
            where TDestination : class, IPropBag;

        long ClearEmittedTypeCache();

        void ClearCaches();
    }


}