using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperCache
    {
        IAutoMapperRequestKeyGen RegisterRawAutoMapperRequest(IAutoMapperRequestKeyGen mapperRequest);
        IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
        long ClearRawAutoMappersCache();
    }
}
