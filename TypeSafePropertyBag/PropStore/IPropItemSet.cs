using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    public interface IPropItemSet<L2TRaw>
    {
        int Count { get; }

        void Add(L2TRaw propertyName, IPropItemModel propModelItem);

        bool Contains(L2TRaw propertyName);
        bool Contains(IPropItemModel propModelItem);

        IEnumerable<L2TRaw> GetPropNames();
        IEnumerable<IPropItemModel> GetPropItems();

        bool TryGetPropModelItem(L2TRaw propertyName, out IPropItemModel propModelItem);
        bool TryRemove(L2TRaw propertyName, out IPropItemModel propModelItem);

        void Clear();
    }

}