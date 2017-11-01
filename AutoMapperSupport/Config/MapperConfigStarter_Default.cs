using System;
using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapperConfigStarter_Default : IHaveAMapperConfigurationStep
    {
        public Action<IMapperConfigurationExpression> ConfigurationStep => UseDefaultConfiguration;

        private void UseDefaultConfiguration(IMapperConfigurationExpression cfg)
        {
            // No nothing.
        }
    }
}
