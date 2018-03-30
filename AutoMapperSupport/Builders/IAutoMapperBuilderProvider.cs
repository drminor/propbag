using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IAutoMapperBuilderProvider
    {
        IBuildAutoMapper<TSource, TDestination> GetAutoMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder,
            IAutoMapperService autoMapperService
            )
            where TDestination : class, IPropBag;
    }
}
