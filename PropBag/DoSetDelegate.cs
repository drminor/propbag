using DRM.TypeSafePropertyBag;
using System;


namespace DRM.PropBag
{
    public delegate bool DoSetDelegate(IPropBag target, uint propId, string propertyName, IProp prop, object value);

}
