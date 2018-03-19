using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt64;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    internal interface IPropNodeCollectionShared<L2T, L2TRaw>
    {
        PropItemSetKey<string> PropItemSetKey { get; }

        int Count { get; }
        int MaxPropsPerObject { get; }

        void Add(IPropNodeCollection_Internal<L2T, L2TRaw> sourcePropNodes);

        bool Contains(ExKeyT compKey);
        bool Contains(PropNode propNode);
        bool Contains(ObjectIdType objectId);

        IEnumerable<IPropDataInternal> GetPropDataItems();
        IEnumerable<L2T> GetPropIds();
        IEnumerable<L2TRaw> GetPropNames();
        IEnumerable<PropNode> GetPropNodes();

        bool TryGetPropNode(ExKeyT compKey, out PropNode propNode);
        bool TryGetPropNodeCollection(ObjectIdType objectId, out IPropNodeCollection_Internal<L2T, L2TRaw> propNodeCollection);

        bool TryRemove(IEnumerable<ExKeyT> compKeys);
        bool TryRemove(IPropNodeCollection_Internal<L2T, L2TRaw> sourcePropNodes);

        // General PropId and PropName Support
        int PropertyCount { get; }

        bool DoesPropExist(L2TRaw propertyName);
        bool DoesPropExist(L2T propId);

        bool TryGetPropertyName(L2T propId, out L2TRaw propertyName);
        bool TryGetPropId(L2TRaw propertyName, out L2T propId);
    }
}