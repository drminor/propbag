using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;

    internal interface IHaveTheSimpleKey : IHaveTheKey<CompositeKeyType, ObjectIdType, PropIdType>
    {
        new SimpleExKey GetTheKey(IPropBag propBag, PropIdType propId);

    }
}
