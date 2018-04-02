namespace Swhp.AutoMapperSupport
{
    public class SimpleAutoMapperBuilderProvider : IAutoMapperBuilderProvider
    {
        public IAutoMapperBuilder<TSource, TDestination> GetAutoMapperBuilder<TSource, TDestination>
            (
            IMapperConfigurationBuilder<TSource, TDestination> mapperConfigurationBuilder
            )

        {
            IAutoMapperBuilder<TSource, TDestination> result = new SimpleAutoMapperBuilder<TSource, TDestination>
                (
                    mapperConfigurationBuilder: mapperConfigurationBuilder
                );

            return result;
        }
    }
}
