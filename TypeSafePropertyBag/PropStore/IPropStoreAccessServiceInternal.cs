using DRM.TypeSafePropertyBag.DelegateCaches;
using System;

namespace DRM.TypeSafePropertyBag
{
    // TODO: Replace all L2T propId references with IExplodedKey<UInt64, UInt64, UInt32>;
    internal interface IPropStoreAccessServiceInternal<L2T, L2TRaw> 
    {
        bool TryGetChildPropNode(BagNode sourceBagNode, L2TRaw propertyName, out PropNode child);

        PropNode GetChild(L2T propId);

        IDisposable RegisterHandler<T>(L2T propId, EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);
        bool UnregisterHandler<T>(L2T propId, EventHandler<PcTypedEventArgs<T>> eventHandler);

        IDisposable RegisterHandler(L2T propId, EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);
        bool UnregisterHandler(L2T propId, EventHandler<PcGenEventArgs> eventHandler);

        //WeakRefKey<IPropBag> GetPropBagProxy(StoreNodeProp storeNodeProp);
        //WeakReference<IPropBag> GetPublicInterface(WeakReference<IPropBagInternal> propBagProxy_internal);

        //WeakRefKey<IPropBag> GetPropBagProxy(PropNode storeNodeProp);
        bool TryGetParentPropBagProxy(PropNode storeNodeProp, out WeakRefKey<IPropBag> propBag_wrKey);
    }
}