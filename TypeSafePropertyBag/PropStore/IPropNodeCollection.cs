using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    internal interface IPropNodeCollection
    {
        int Count { get; }
        bool IsFixed { get; }
        int MaxPropsPerObject { get; }

        void Clear();
        void Fix();

        bool Contains(PropNode propNode);
        bool Contains(PropNameType propertyName);
        bool Contains(PropIdType propId);

        //void Add(PropIdType propId, PropNode propNode);
        PropNode CreateAndAdd(IPropDataInternal propData_Internal, PropNameType propertyName, BagNode parent);

        IEnumerable<IPropDataInternal> GetPropDataItems();
        IEnumerable<PropNode> GetPropNodes();

        bool TryGetPropertyName(PropIdType propId, out PropNameType propertyName);
        bool TryGetPropId(PropNameType propertyName, out PropIdType propId);

        bool TryGetPropNode(PropNameType propertyName, out PropNode propNode);
        bool TryGetPropNode(PropIdType propId, out PropNode propNode);

        bool TryRemove(PropIdType propId, out PropNode propNode);
    }
}