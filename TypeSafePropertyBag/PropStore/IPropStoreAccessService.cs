using DRM.TypeSafePropertyBag.Fundamentals;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropStoreAccessService<PropBagT, PropDataT>
        where PropBagT : IPropBag
        where PropDataT : IPropGen
    {
        PropDataT this[PropBagT propBag, uint propId] { get; set; }

        int MaxPropsPerObject { get; }
        long MaxObjectsPerAppDomain { get; }

        void Clear(PropBagT propBag);
        bool ContainsKey(PropBagT propBag, uint propId);

        IEnumerator<KeyValuePair<string, PropDataT>> GetEnumerator(PropBagT propBag);
        IEnumerable<string> GetKeys(PropBagT propBag);
        IEnumerable<PropDataT> GetValues(PropBagT propBag);

        bool TryAdd(PropBagT propBag, uint propId, PropDataT propData);
        bool TryGetValue(PropBagT propBag, uint propId, out PropDataT propData);
        bool TryRemove(PropBagT propBag, uint propId, out PropDataT propData);
    }
}