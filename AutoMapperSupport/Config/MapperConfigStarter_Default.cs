using System;
using AutoMapper;

namespace Swhp.AutoMapperSupport
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
