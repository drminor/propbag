using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IPropBagMapperBuilderProvider
    {
        IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IPropBagMapperService propBagMapperService
            )
            where TDestination : class, IPropBag;
    }
}
