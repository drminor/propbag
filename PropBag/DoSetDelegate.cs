using DRM.TypeSafePropertyBag;
using System;


namespace DRM.PropBag
{
    internal delegate bool DoSetDelegate(IPropBag target, uint propId, string propertyName, IProp prop, object value);

    // TODO: use the IPropBag interface instead of PropBag (concrete implementation.)
    //internal delegate IList GetTypedCollectionDelegate(PropBag source, string propertyName);
}
