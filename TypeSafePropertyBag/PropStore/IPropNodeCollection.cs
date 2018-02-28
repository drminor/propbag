﻿using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    internal interface IPropNodeCollection_Internal<L2T, L2TRaw> : IPropNodeCollection<L2T, L2TRaw>
    {
        bool Contains(PropNode propNode);
        PropNode CreateAndAdd(IPropDataInternal propData_Internal, L2TRaw propertyName, BagNode parent);

        IEnumerable<PropNode> GetPropNodes();

        IEnumerable<IPropDataInternal> GetPropDataItems();

        bool TryGetPropNode(L2TRaw propertyName, out PropNode propNode);
        bool TryGetPropNode(L2T propId, out PropNode propNode);

        bool TryRemove(L2T propId, out PropNode propNode);

        //long GetNextGenerationId();
    }

    public interface IPropNodeCollection<L2T, L2TRaw>
    {
        int Count { get; }
        bool IsFixed { get; }
        int MaxPropsPerObject { get; }

        void Clear();
        void Fix();

        bool Contains(L2TRaw propertyName);
        bool Contains(L2T propId);

        IEnumerable<L2TRaw> GetPropNames();
        IEnumerable<L2T> GetPropIds();

        IReadOnlyDictionary<L2TRaw, IPropData> GetPropDataItemsDict();

        bool TryGetPropertyName(L2T propId, out L2TRaw propertyName);
        bool TryGetPropId(L2TRaw propertyName, out L2T propId);
    }

}