using System;

namespace DRM.TypeSafePropertyBag
{
    internal interface IPropStoreAccessServiceInternal<L2T, L2TRaw> 
    {
        IL2KeyMan<L2T, L2TRaw> Level2KeyManager { get; }

        bool TryGetChildPropNode(StoreNodeBag sourceBagNode, L2TRaw propertyName, out StoreNodeProp child);

        StoreNodeProp GetChild(L2T propId);

        IDisposable RegisterHandler<T>(L2T propId, EventHandler<PcTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);

        IDisposable RegisterHandler(L2T propId, EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);
    }
}