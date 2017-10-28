using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapperConfigurationStepWithTypes<TSource, TDestination>
    {
        Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> ConfigurationStep { get; }
    }
}
