
using System;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    //internal interface IReceivePropStoreNodeUpdates<T> : IReceivePropStoreNodeUpdates_PropBag<T>, IReceivePropStoreNodeUpdates_PropNode<T>
    //{

    //}

    internal interface IReceivePropStoreNodeUpdates_PropBag<T>
    {
        void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent, T oldValue);
        void OnPropStoreNodeUpdated(WeakReference<IPropBag> propItemParent);
    }

    internal interface IReceivePropStoreNodeUpdates_PropNode<T>
    {
        void OnPropStoreNodeUpdated(StoreNodeProp propNode, T oldValue);
        void OnPropStoreNodeUpdated(StoreNodeProp propNode);
    }

    internal interface IReceivePropStoreNodeUpdates_Value<T>
    {
        void OnPropStoreNodeUpdated(PcTypedEventArgs<T> e);
    }
}
