
using System;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    internal interface IReceivePropStoreNodeUpdates<T> : IReceivePropStoreNodeUpdates
    {
        //void OnPropStoreNodeUpdated(StoreNodeProp newNode, bool OldValueIsUndefined, T oldValue);
        void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent, bool OldValueIsUndefined, T oldValue);
    }

    internal interface IReceivePropStoreNodeUpdates
    {
        //void OnPropStoreNodeUpdated(StoreNodeProp newNode, bool OldValueIsUndefined, object oldValue);

        // TODO: Consider having this method return bool -- True = success; False = failure.
        void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent, bool OldValueIsUndefined, object oldValue);
    }
}
