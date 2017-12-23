using System;
using System.Collections.Generic;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropStoreAccessService<L2T, L2TRaw> : IRegisterSubscriptions<L2T>, IRegisterBindings<L2T>, IDisposable
    {
        int PropertyCount { get; }

        bool TryGetPropId(L2TRaw propertyName, out L2T propId);
        bool TryGetPropName(L2T propertyId, out L2TRaw propertyName);
        L2T Add(L2TRaw propertyName);

        // IDictionary-Like Methods
        IPropData this[IPropBag propBag, L2T propId] { get; }

        bool ContainsKey(IPropBag propBag, L2T propId);

        IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropData>> GetEnumerator(IPropBag propBag);
        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropData> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, out IPropData propData);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, EventHandler<PcGenEventArgs> handler, SubscriptionPriorityGroup priorityGroup, out IPropData propData);
        bool TryAdd<PropT>(IPropBag propBag, L2T propId, IProp genericTypedProp, EventHandler<PcTypedEventArgs<PropT>> handler, SubscriptionPriorityGroup priorityGroup, out IPropData propData);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, object target, MethodInfo method, SubscriptionKind subscriptionKind, SubscriptionPriorityGroup priorityGroup, out IPropData propData);

        bool TryGetValue(IPropBag propBag, L2T propId, out IPropData propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropData propData);

        // Restricted Update Method on Dictionary-like object.
        bool SetTypedProp(IPropBag propBag, L2T propId, L2TRaw propertyName, IProp genericTypedProp);

        IPropStoreAccessService<L2T, L2TRaw> CloneProps(IPropBag callingPropBag, IPropBag copySource);

        int ClearAllProps(IPropBag propBag);

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }
    }
}