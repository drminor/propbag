
using DRM.TypeSafePropertyBag;

namespace DRM.PropBag.AutoMapperSupport
{
    public interface ICachePropBagMappers
    {
        IPropBagMapperKeyGen RegisterPropBagMapperRequest(IPropBagMapperKeyGen mapperRequest);

        IPropBagMapperGen GetPropBagMapper(IPropBagMapperKeyGen mapperRequest);

        IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
        (
            IPropBagMapperKey<TSource, TDestination> mapperRequest
        )
        where TDestination : class, IPropBag;

        long ClearPropBagMappersCache();
    }
}
