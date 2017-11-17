using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    //using L1T = UInt32;

    // TODO: Consider renaming this interface to: IGuardPropStore.
    public interface IPropStoreAccessService<L2T, L2TRaw> : IRegisterSubscriptions<L2T>
    {
        // IDictionary-Like Methods
        IPropData this[IPropBag propBag, L2T propId] { get; }

        void Clear(IPropBag propBag);
        bool ContainsKey(IPropBag propBag, L2T propId);

        IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropData>> GetEnumerator(IPropBag propBag);
        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropData> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2T propId, IProp genericTypedProp, out IPropData propData);

        bool TryGetValue(IPropBag propBag, L2T propId, out IPropData propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropData propData);

        // Restricted Update Method on Dictionary-like object.
        bool SetTypedProp(IPropBag propBag, L2T propId, IProp genericTypedProp);

        // Diagnostics
        void IncAccess();
        int AccessCounter { get; }
        // For testing only??
        //L1T GetParentObjectId(IPropBag propBag);
    }
}