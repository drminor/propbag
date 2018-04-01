using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    public interface ICacheAutoMappers
    {
        IAutoMapperRequestKeyGen RegisterRawAutoMapperRequest(IAutoMapperRequestKeyGen mapperRequest);
        IMapper GetRawAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
        long ClearRawAutoMappersCache();
    }
}
