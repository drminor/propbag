using System;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IProvideAutoMappers : ICachePropBagMappers
    {
        bool HasPropModelLookupService { get; }

        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>(string propModelResourceKey, Type typeToWrap, string configPackageName, IHaveAMapperConfigurationStep configStarterForThisRequest = null, IPropFactory propFactory = null) where TDestination : class, IPropBag;
        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>(IPropModel propModel, Type typeToWrap, string configPackageName, IHaveAMapperConfigurationStep configStarterForThisRequest = null, IPropFactory propFactory = null) where TDestination : class, IPropBag;

        // The PropModel supplies the TDestination type and the Type that will be wrapped.
        IPropBagMapperKeyGen RegisterMapperRequest(string propModelResourceKey, Type sourceType, string configPackageName);
        IPropBagMapperKeyGen RegisterMapperRequest(IPropModel propModel, Type sourceType, string configPackageName);

        IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest) where TDestination : class, IPropBag;
    }

    public interface ICachePropBagMappers
    {
        IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapperRequest);

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapperRequest);
    }
}