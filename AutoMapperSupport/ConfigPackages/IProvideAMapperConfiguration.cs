using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IProvideAMapperConfiguration
    {
        IConfigureAMapper<TSource, TDestination> GetTheMapperConfig<TSource, TDestination>() where TDestination : class, IPropBag;
    }
}
