using System;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapperConfigStarter_Empty : IMapperConfigurationStepGen
    {
        public Action<IMapperConfigurationExpression> ConfigurationStep => ResetMemberConfiguration;

        private void ResetMemberConfiguration(IMapperConfigurationExpression cfg)
        {
            // This will reset the new MapperConfig so that there are no default settings.
            cfg.AddMemberConfiguration();
        }
    }
}
