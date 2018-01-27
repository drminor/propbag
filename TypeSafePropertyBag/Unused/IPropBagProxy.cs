using System;

namespace DRM.TypeSafePropertyBag.Unused
{
    internal interface IPropBagProxy
    {
        WeakReference<IPropBagInternal> PropBagRef { get; }
        //ObjectIdType ObjectId { get; }
        //L2KeyManType Level2KeyManager { get; }
    }
}
