using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface IGetInitialMapperConfig
    {
        IConfigurationProvider GetNewBaseConfiguration();
    }
}
