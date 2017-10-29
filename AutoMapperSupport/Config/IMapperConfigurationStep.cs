using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapperConfigurationStepGen
    {
        Action<IMapperConfigurationExpression> ConfigurationStep { get; }
    }

    public interface IMapperConfigurationStep<TSource, TDestination> where TDestination : class, IPropBag
    {
        Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> ConfigurationStep { get; }
    }
}
