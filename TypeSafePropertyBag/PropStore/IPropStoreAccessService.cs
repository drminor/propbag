using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    // TODO: Consider renaming this interface to: IGuardPropStore.
    public interface IPropStoreAccessService<L2T, L2TRaw>
    {
        IPropGen this[IPropBag propBag, L2T propId] { get; set; }

        int MaxPropsPerObject { get; }
        long MaxObjectsPerAppDomain { get; }

        void Clear(IPropBag propBag);
        bool ContainsKey(IPropBag propBag, L2T propId);

        IEnumerable<KeyValuePair<L2TRaw, IPropGen>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<L2TRaw, IPropGen>> GetEnumerator(IPropBag propBag);
        IEnumerable<L2TRaw> GetKeys(IPropBag propBag);
        IEnumerable<IPropGen> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, L2T propId, IPropGen propData);
        bool TryGetValue(IPropBag propBag, L2T propId, out IPropGen propData);
        bool TryRemove(IPropBag propBag, L2T propId, out IPropGen propData);
    }
}