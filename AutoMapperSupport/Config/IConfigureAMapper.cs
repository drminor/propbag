using AutoMapper;
using System;
using System.Collections.Generic;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IConfigureAMapper<TSource, TDestination> : IConfigureAMapperGen where TDestination : class, IPropBag
    {
        Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> FinalConfigAction { get; set; }

        IConfigurationProvider GetConfigurationProvider(IPropBagMapperKey<TSource, TDestination> propBagMapperKey);

        IConfigurationProvider GetConfigurationProvider(IPropBagMapperKey<TSource, TDestination> propBagMapperKey,
            IMapperConfigurationStepGen configStarter);
    }

    public interface IConfigureAMapperGen
    {
        bool SupportsMapFrom { get; }
        IList<IMapperConfigurationStepGen> ConfigurationSteps { get; }

        void Add(IMapperConfigurationStepGen step);
        void Clear();

        IConfigurationProvider GetConfigurationProviderGen();
    }
}
