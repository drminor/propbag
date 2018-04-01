//using DRM.TypeSafePropertyBag;

namespace Swhp.AutoMapperSupport
{
    public interface IProvideAMapperConfiguration
    {
        IConfigureAMapper<TSource, TDestination> GetTheMapperConfig<TSource, TDestination>(); //where TDestination : class, IPropBag;
    }

    public interface IMapperConfigurationLookupService
    {
        IConfigureAMapper<TSource, TDestination> GetTheMapperConfig<TSource, TDestination>(string key); //where TDestination : class, IPropBag;
    }
}
