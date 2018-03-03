using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropItemSet<L2TRaw>
    {
        int Count { get; }

        void Add(L2TRaw propertyName, IPropModelItem propModelItem);

        bool Contains(L2TRaw propertyName);
        bool Contains(IPropModelItem propModelItem);

        IEnumerable<L2TRaw> GetPropNames();
        IEnumerable<IPropModelItem> GetPropItems();

        bool TryGetPropModelItem(L2TRaw propertyName, out IPropModelItem propModelItem);
        bool TryRemove(L2TRaw propertyName, out IPropModelItem propModelItem);

        void Clear();
    }

}