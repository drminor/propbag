using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public class MapperConfigStarter_Empty : IGetInitialMapperConfig
    {
        public IConfigurationProvider GetNewBaseConfiguration()
        {
            IConfigurationProvider result = new MapperConfiguration(ResetMemberConfiguration);
            return result;
        }

        private void ResetMemberConfiguration(IMapperConfigurationExpression cfg)
        {
            // This will reset the new MapperConfig so that there are no default settings.
            cfg.AddMemberConfiguration();
        }
    }
}
