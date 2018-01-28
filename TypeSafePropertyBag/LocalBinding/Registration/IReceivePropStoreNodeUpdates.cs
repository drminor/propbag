
using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag.LocalBinding
{
    //internal interface IReceivePropStoreNodeUpdates<T> : IReceivePropStoreNodeUpdates_PropBag<T>, IReceivePropStoreNodeUpdates_PropNode<T>
    //{

    //}

    internal interface IReceivePropStoreNodeUpdates_PropBag<T>
    {
        void OnPropStoreNodeUpdated(WeakRefKey<IPropBag> propItemParent, WeakReference<IPropBag> oldPropItemParent);
        void OnPropStoreNodeUpdated(WeakRefKey<IPropBag> propItemParent);
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
