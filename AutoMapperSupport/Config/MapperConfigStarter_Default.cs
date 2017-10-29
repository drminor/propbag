using System;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapperConfigStarter_Default : IMapperConfigurationStepGen
    {
        public Action<IMapperConfigurationExpression> ConfigurationStep => UseDefaultConfiguration;

        private void UseDefaultConfiguration(IMapperConfigurationExpression cfg)
        {
            // No nothing.
        }
    }
}
