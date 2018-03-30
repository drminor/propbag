using AutoMapper;
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IBuildMapperConfigurations<TSource, TDestination> /*: IBuildMapperConfigurationsGen*/ //where TDestination : class, IPropBag
    {
        //IConfigurationProvider GetNewConfiguration
        //    (
        //    //IConfigureAMapper<TSource, TDestination> configs,
        //    IPropBagMapperKey<TSource, TDestination> mapRequest
        //    /*, IHaveAMapperConfigurationStep configStarter = null*/
        //    );

        IConfigurationProvider GetNewConfiguration
            (
            //IConfigureAMapper<TSource, TDestination> configs,
            IAutoMapperRequestKey<TSource, TDestination> mapRequest
            /*, IHaveAMapperConfigurationStep configStarter = null*/
            );
    }

    public interface IBuildMapperConfigurationsGen
    {
        //IConfigurationProvider GetNewConfiguration
        //    (
        //    IConfigureAMapperGen configs,
        //    IHaveAMapperConfigurationStep configStarter = null
        //    );
    }
}
