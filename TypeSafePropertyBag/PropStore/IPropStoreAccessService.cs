using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropStoreAccessService<L2T, L2TRaw> : IRegisterSubscriptions<L2T>, IRegisterBindings<L2T>, IDisposable
    {
        IL2KeyMan<L2T,L2TRaw> Level2KeyManager { get; }

        // IDictionary-Like Methods
        IPropData this[IPropBag propBag, L2T propId] { get; }

        bool ContainsKey(IPropBag propBag, L2T propId);

        IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropData>> GetEnumerator(IPropBag propBag);
        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropData> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2T propId, L2TRaw propertyName, IProp genericTypedProp, out IPropData propData);

        bool TryGetValue(IPropBag propBag, L2T propId, out IPropData propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropData propData);

        // Restricted Update Method on Dictionary-like object.
        bool SetTypedProp(IPropBag propBag, L2T propId, L2TRaw propertyName, IProp genericTypedProp);

        IPropStoreAccessService<L2T, L2TRaw> CloneProps(IPropBag callingPropBag, IPropBag copySource);

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }
    }
}