using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IPropBagMapperBuilderProvider
    {
        //ICreateWrapperTypes WrapperTypeCreator { get; }

        IBuildPropBagMapper<TSource, TDestination> GetPropBagMapperBuilder<TSource, TDestination>
            (
            IBuildMapperConfigurations<TSource, TDestination> mapperConfigurationBuilder
            //,
            //IProvideAutoMappers autoMapperService
            )
            where TDestination : class, IPropBag;

        //long ClearTypeCache();
    }
}
