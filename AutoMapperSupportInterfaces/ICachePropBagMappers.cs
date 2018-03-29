using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICachePropBagMappers
    {
        IPropBagMapperKeyGen RegisterPropBagMapperRequest(IPropBagMapperKeyGen mapperRequest);

        IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapperRequest);

        long ClearPropBagMappersCache();
    }
}
