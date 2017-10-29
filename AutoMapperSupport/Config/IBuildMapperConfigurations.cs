using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IBuildMapperConfigurationsGen
    {
        IConfigurationProvider GetNewBaseConfiguration(IConfigureAMapperGen configs);
        IConfigurationProvider GetNewBaseConfiguration(IConfigureAMapperGen configs, IMapperConfigurationStepGen configStarter);
    }

    public interface IBuildMapperConfigurations<TSource, TDestination> : IBuildMapperConfigurationsGen where TDestination : class, IPropBag
    {
        IConfigurationProvider GetNewBaseConfiguration(IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest);

        IConfigurationProvider GetNewBaseConfiguration(IConfigureAMapper<TSource, TDestination> configs,
            IPropBagMapperKey<TSource, TDestination> mapRequest,
            IMapperConfigurationStepGen configStarter);
    }
}
