﻿
namespace DRM.TypeSafePropertyBag
{
    public interface ICachePropBagMappers
    {
        IPropBagMapperRequestKeyGen RegisterPropBagMapperRequest(IPropBagMapperRequestKeyGen mapperRequest);

        IPropBagMapperGen GetPropBagMapper(IPropBagMapperRequestKeyGen mapperRequest);

        IPropBagMapper<TSource, TDestination> GetPropBagMapper<TSource, TDestination>
        (
            IPropBagMapperRequestKey<TSource, TDestination> mapperRequest
        )
        where TDestination : class, IPropBag;

        long ClearPropBagMappersCache();
    }
}
