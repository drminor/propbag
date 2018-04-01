using AutoMapper;
using System;

namespace Swhp.AutoMapperSupport
{
    public interface IMapperConfigurationExpressionProvider<TSource, TDestination> //where TDestination : class, IPropBag
    {
        Action<IAutoMapperRequestKey<TSource, TDestination>, IMapperConfigurationExpression> ActionStep { get; }

        bool RequiresProxyType { get; }
    }
}
