using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    // TODO: Consider renaming this interface to: IGuardPropStore.
    public interface IPropStoreAccessService<PropIdT, PropNameT>
    {
        IPropGen this[IPropBag propBag, PropIdT propId] { get; set; }

        int MaxPropsPerObject { get; }
        long MaxObjectsPerAppDomain { get; }

        void Clear(IPropBag propBag);
        bool ContainsKey(IPropBag propBag, PropIdT propId);

        IEnumerable<KeyValuePair<PropNameT, IPropGen>> GetCollection(IPropBag propBag);
        IEnumerator<KeyValuePair<PropNameT, IPropGen>> GetEnumerator(IPropBag propBag);
        IEnumerable<PropNameT> GetKeys(IPropBag propBag);
        IEnumerable<IPropGen> GetValues(IPropBag propBag);

        bool TryAdd(IPropBag propBag, PropIdT propId, IPropGen propData);
        bool TryGetValue(IPropBag propBag, PropIdT propId, out IPropGen propData);
        bool TryRemove(IPropBag propBag, PropIdT propId, out IPropGen propData);
    }
}