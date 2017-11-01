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
            IHaveAMapperConfigurationStep configStarter);
    }

    public interface IConfigureAMapperGen
    {
        bool SupportsMapFrom { get; }
        IList<IHaveAMapperConfigurationStep> ConfigurationSteps { get; }

        void Add(IHaveAMapperConfigurationStep step);
        void Clear();

        IConfigurationProvider GetConfigurationProviderGen();
    }
}
