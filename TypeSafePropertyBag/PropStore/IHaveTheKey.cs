using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;

    public interface IHaveTheKey<PropBagT>
    {
        SimpleExKey GetTheKey(PropBagT propBag, PropIdType propId);

    }
}
