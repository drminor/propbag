using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using L1T = UInt32;

    // TODO: Consider renaming this interface to: IGuardPropStore.
    public interface IPropStoreAccessService<L2T, L2TRaw>
    {
        IPropData this[IPropBag propBag, L2T propId] { get; /*set;*/ }

        long MaxObjectsPerAppDomain { get; }
        int MaxPropsPerObject { get; }

        void Clear(IPropBag propBag);
        bool ContainsKey(IPropBag propBag, L2T propId);

        IEnumerable<KeyValuePair<L2TRaw, IPropData>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropData>> GetEnumerator(IPropBag propBag);
        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropData> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2T propId, IPropData propData);
        bool TryGetValue(IPropBag propBag, L2T propId, out IPropData propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropData propData);

        bool SetChildObjectId(IPropBag propBag, L2T propId, L1T childPropId);
        bool SetTypedProp(IPropBag propBag, L2T propId, IProp genericTypedProp);

        // For testing only??
        uint GetParentObjectId(IPropBag propBag);
    }
}