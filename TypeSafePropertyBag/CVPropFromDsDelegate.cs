using System;

namespace DRM.TypeSafePropertyBag
{
    using PropNameType = String;

    public delegate IProp CVPropFromDsDelegate(IPropBag target, PropNameType propertyName, PropNameType srcPropName, IMapperRequest mr);
}
