using System;
using DRM.PropBag.ControlModel;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IProvideAutoMappers
    {
        bool HasPropModelLookupService { get; }

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapperRequest);
        IPropBagMapper<TSource, TDestination> GetMapper<TSource, TDestination>(IPropBagMapperKey<TSource, TDestination> mapperRequest) where TDestination : class, IPropBag;
        IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapperRequest);
        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>(PropModel propModel, Type targetType, string configPackageName, IHaveAMapperConfigurationStep configStarterForThisRequest = null, IPropFactory propFactory = null) where TDestination : class, IPropBag;
        IPropBagMapperKey<TSource, TDestination> RegisterMapperRequest<TSource, TDestination>(string resourceKey, Type targetType, string configPackageName, IHaveAMapperConfigurationStep configStarterForThisRequest = null, IPropFactory propFactory = null) where TDestination : class, IPropBag;
    }
}