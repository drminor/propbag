using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapperConfigStarter_Default : IGetInitialMapperConfig
    {
        public IConfigurationProvider GetNewBaseConfiguration()
        {
            IConfigurationProvider result = new MapperConfiguration(UseDefaultConfiguration);
            return result;
        }

        private void UseDefaultConfiguration(IMapperConfigurationExpression cfg)
        {
            // No nothing.
        }
    }
}
