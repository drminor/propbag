using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICachePropBagMappers
    {
        IPropBagMapperKeyGen RegisterMapperRequest(IPropBagMapperKeyGen mapperRequest);

        IPropBagMapperGen GetMapper(IPropBagMapperKeyGen mapperRequest);

        long ClearMappersCache();
    }
}
