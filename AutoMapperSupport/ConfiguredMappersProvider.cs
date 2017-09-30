using AutoMapper;
using System;
using System.Threading;

namespace DRM.PropBag.AutoMapperSupport
{
    public class CachesProvider
    {
        #region Private Backing Members
        static Lazy<ConfiguredMappers> theSingleConfiguredMappersCacheExtraMembers;
        static Lazy<ConfiguredMappers> theSingleConfiguredMappersCacheEmitProxy;
        #endregion

        #region Public Accessors
        // TypeDesc<T>-based Converter Cache
        public static ConfiguredMappers GetMapperCache(PropBagMappingStrategyEnum mappingStrategy)
        {
            switch(mappingStrategy)
            {
                case PropBagMappingStrategyEnum.ExtraMembers:
                    {
                        return theSingleConfiguredMappersCacheExtraMembers.Value;
                    }
                case PropBagMappingStrategyEnum.EmitProxy:
                    {
                        return theSingleConfiguredMappersCacheEmitProxy.Value;
                    }
                default:
                    {
                        throw new ApplicationException("That mapping strategy is not supported, or unrecognized.");
                    }
            }
        }
        #endregion

        #region The Static Constructor
        static CachesProvider()
        {
            // Create the static instances upon first reference.

            // Configured Mappers Cache using the ExtraMembers Strategy
            theSingleConfiguredMappersCacheExtraMembers
                = new Lazy<ConfiguredMappers>(() => CreateConfiguredMappersCache(PropBagMappingStrategyEnum.ExtraMembers), LazyThreadSafetyMode.ExecutionAndPublication);

            theSingleConfiguredMappersCacheEmitProxy
                = new Lazy<ConfiguredMappers>(() => CreateConfiguredMappersCache(PropBagMappingStrategyEnum.EmitProxy), LazyThreadSafetyMode.ExecutionAndPublication);

        }
        #endregion

        #region Constructor Support
        private static ConfiguredMappers CreateConfiguredMappersCache(PropBagMappingStrategyEnum mappingStrategy)
        {
            Func<Action<IMapperConfigurationExpression>, IConfigurationProvider> configBuilder
                = new MapperConfigurationProvider().BaseConfigBuilder;

            MapperConfigInitializerProvider mapperConfigExpression
                = new MapperConfigInitializerProvider(mappingStrategy);

            ConfiguredMappers result = new ConfiguredMappers(configBuilder, mapperConfigExpression);

            return result;
        }
        #endregion

        #region Instance Constructors
        // Mark as private to disallow instances of this class to be created.
        private CachesProvider() { }
        #endregion
    }
}
