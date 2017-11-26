using System;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    #endregion

    public interface IPropBagProxy
    {
        WeakReference<IPropBagInternal> PropBagRef { get; }
        //ObjectIdType ObjectId { get; }
        L2KeyManType Level2KeyManager { get; }
    }
}
