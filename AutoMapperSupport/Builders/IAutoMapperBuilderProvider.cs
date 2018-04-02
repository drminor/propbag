
namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperBuilderProvider
    {
        IAutoMapperBuilder<TSource, TDestination> GetAutoMapperBuilder<TSource, TDestination>
        (
            IMapperConfigurationBuilder<TSource, TDestination> mapperConfigurationBuilder
        );
    }
}
