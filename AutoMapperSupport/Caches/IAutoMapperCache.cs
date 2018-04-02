using AutoMapper;

namespace Swhp.AutoMapperSupport
{
    public interface IAutoMapperCache
    {
        IAutoMapperRequestKeyGen RegisterAutoMapperRequest(IAutoMapperRequestKeyGen mapperRequest);
        IMapper GetAutoMapper(IAutoMapperRequestKeyGen mapperRequest);
        long ClearTheAutoMappersCache();
    }
}
