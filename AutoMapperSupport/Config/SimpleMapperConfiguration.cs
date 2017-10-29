using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport.Config
{
    public class SimpleMapperConfiguration<TSource, TDestination>
        : SimpleMapperConfigurationGenBase, IConfigureAMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        // In our constructor, we give the base class the supplied (typed) mapperConfigBuilder.
        // The base class references it as a IBuildMapperConfigurationsGen.
        // Here we return this value as a (typed) IBuildMapperConfiguration.
        private IBuildMapperConfigurations<TSource, TDestination> MapperConfigBuilder => (IBuildMapperConfigurations<TSource, TDestination>)base.MapperConfigBuilderGen;

        public IMapperConfigurationStep<TSource, TDestination> FinalConfigStep { get; set; }

        public SimpleMapperConfiguration(IBuildMapperConfigurations<TSource, TDestination> mapperConfigBuilder)
            : base(mapperConfigBuilder)
        {
        }

        public IConfigurationProvider GetConfigurationProvider(IPropBagMapperKey<TSource, TDestination> propBagMapperKey)
        {
            IConfigurationProvider result = MapperConfigBuilder.GetNewBaseConfiguration(this);
            return result;
        }
    }
}
