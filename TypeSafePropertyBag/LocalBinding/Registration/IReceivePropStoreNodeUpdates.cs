
using System;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    internal interface IReceivePropStoreNodeUpdates<T>
    {
        //void OnPropStoreNodeUpdated(StoreNodeProp newNode, bool OldValueIsUndefined, T oldValue);

        void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent, T oldValue);
        void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent, bool OldValueIsUndefined);

        void OnPropStoreNodeUpdated(StoreNodeProp propNode, T oldValue);
        void OnPropStoreNodeUpdated(StoreNodeProp propNode, bool OldValueIsUndefined);

        // TODO: Consider including methods that take the new value.
    }
}
