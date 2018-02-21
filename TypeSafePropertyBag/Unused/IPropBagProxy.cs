using System;

namespace DRM.TypeSafePropertyBag.Unused
{
    internal interface IPropBagProxy
    {
        WeakReference<IPropBagInternal> PropBagRef { get; }
    }
}
