using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IMapperConfigurationFinalAction<TSource, TDestination> where TDestination : class, IPropBag
    {
        Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> ActionStep { get; }
    }
}
