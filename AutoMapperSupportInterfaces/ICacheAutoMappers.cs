using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICacheAutoMappers
    {
        IPropBagMapperKeyGen RegisterRawAutoMapperRequest(IPropBagMapperKeyGen mapperRequest);

        IMapper GetRawAutoMapper(IPropBagMapperKeyGen mapperRequest);

        long ClearMappersCache();
    }
}
