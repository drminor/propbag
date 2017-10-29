﻿using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IBuildMapperConfigurationsGen
    {
        IConfigurationProvider GetNewConfiguration(IConfigureAMapperGen configs);
        IConfigurationProvider GetNewConfiguration(IConfigureAMapperGen configs, IMapperConfigurationStepGen configStarter);
    }

    public interface IBuildMapperConfigurations<TSource, TDestination> : IBuildMapperConfigurationsGen where TDestination : class, IPropBag
    {
        IConfigurationProvider GetNewConfiguration(IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest);

        IConfigurationProvider GetNewConfiguration(IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapperConfigurationStepGen configStarter);
    }
}
