using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public delegate bool DoSetDelegate(IPropBag target, PropIdType propId, PropNameType propertyName, IProp prop, object value);
}
