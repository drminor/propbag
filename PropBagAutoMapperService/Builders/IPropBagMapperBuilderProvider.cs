
namespace DRM.TypeSafePropertyBag
{
    public interface IPropBagMapperBuilderProvider
    {
        IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            //IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IPropBagMapperService propBagMapperService
            )
            where TDestination : class, IPropBag;
    }
}
