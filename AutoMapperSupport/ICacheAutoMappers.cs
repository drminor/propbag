using AutoMapper;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICacheAutoMappers
    {
        IAutoMapperRequestKeyGen RegisterRawAutoMapperRequest(IAutoMapperRequestKeyGen mapperRequest);
        IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
        long ClearRawAutoMappersCache();
    }
}
