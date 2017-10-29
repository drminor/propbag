using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class SimpleMapperConfiguration<TSource, TDestination>
        : SimpleMapperConfigurationGenBase, IConfigureAMapper<TSource, TDestination> where TDestination : class, IPropBag
    {
        // In our constructor, we give the base class the supplied (typed) mapperConfigBuilder.
        // The base class references it as a IBuildMapperConfigurationsGen.
        // Here we return this value as a (typed) IBuildMapperConfiguration.
        private IBuildMapperConfigurations<TSource, TDestination> MapperConfigBuilder
            => (IBuildMapperConfigurations<TSource, TDestination>)base.MapperConfigBuilderGen;

        public Action<IPropBagMapperKey<TSource, TDestination>, IMapperConfigurationExpression> FinalConfigAction { get; set; }

        public IMapperConfigurationStepGen ConfigStarter { get; set; }

        #region Constructor
        public SimpleMapperConfiguration(IBuildMapperConfigurations<TSource, TDestination> mapperConfigBuilder, IMapperConfigurationStepGen configStarter = null)
            : base(mapperConfigBuilder)
        {
            ConfigStarter = configStarter;
        }
        #endregion

        public IConfigurationProvider GetConfigurationProvider(IPropBagMapperKey<TSource, TDestination> propBagMapperKey) 
        {
            return GetConfigurationProvider(propBagMapperKey, ConfigStarter);
        }

        public IConfigurationProvider GetConfigurationProvider(IPropBagMapperKey<TSource, TDestination> propBagMapperKey,
            IMapperConfigurationStepGen configStarter)
        {
            IConfigurationProvider result = MapperConfigBuilder.GetNewConfiguration(this, propBagMapperKey);
            return result;
        }
    }
}
