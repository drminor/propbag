using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IProvideAutoMappers : ICachePropBagMappers
    {
        bool HasPropModelLookupService { get; }

        IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest) where TDestination : class, IPropBag;
        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>(PropModel propModel, Type targetType, string configPackageName, IHaveAMapperConfigurationStep configStarterForThisRequest = null, IPropFactory propFactory = null) where TDestination : class, IPropBag;
        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>(string resourceKey, Type targetType, string configPackageName, IHaveAMapperConfigurationStep configStarterForThisRequest = null, IPropFactory propFactory = null) where TDestination : class, IPropBag;

        IPropBagMapperKeyGen RegisterMapperRequest(string propModelResourceKey, Type sourceType, string configPackageName);
    }

    public interface ICachePropBagMappers
    {
        IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapperRequest);

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapperRequest);
    }
}