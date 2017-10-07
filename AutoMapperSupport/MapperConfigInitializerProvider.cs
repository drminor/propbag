using AutoMapper;
using System;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapperConfigInitializerProvider : IInitializeAMapperConf
    {
        public Action<IMapperConfigurationExpression> InitialConfigurationAction { get; }

        public MapperConfigInitializerProvider(PropBagMappingStrategyEnum mappingStrategy)
        {
            switch (mappingStrategy)
            {
                case PropBagMappingStrategyEnum.EmitProxy:
                    {
                        InitialConfigurationAction = BuildEmitProxyConfig;
                        break;
                    }
                case PropBagMappingStrategyEnum.ExtraMembers:
                    {
                        InitialConfigurationAction = new MapperConfigForExtraMembers().InitialConfigurationAction;
                        break;
                    }
                default:
                    {
                        throw new ApplicationException($"{nameof(mappingStrategy)} has value {mappingStrategy} which is not supported or unexpected.");
                    }
            }

        }

        private void BuildEmitProxyConfig(IMapperConfigurationExpression cfg)
        {
            // There is nothing special to do to support the use of emittted proxies.
        }
    }

}
