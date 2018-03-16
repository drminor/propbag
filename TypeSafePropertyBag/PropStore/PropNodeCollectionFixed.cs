using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PropModelType = IPropModel<String>;

    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;

    internal class PropNodeCollectionFixed : PropNodeCollectionIntInterface/*, IEquatable<PropItemSetInternalInterface>*/
    {
        #region Private Members

        WeakRefKey<PropModelType>? _propItemSetId;

        readonly PropNode[] _children;

        Dictionary<PropNameType, PropNode> PropItemsByName;

        object _sync = new object();

        #endregion

        #region Constructor

        public PropNodeCollectionFixed(PropNodeCollectionIntInterface sourcePropNodes)
            : this(sourcePropNodes.GetPropNodes(), CheckPropItemSetId(sourcePropNodes.PropItemSetId), sourcePropNodes.MaxPropsPerObject)
        {
        }

        public PropNodeCollectionFixed(PropNodeCollectionIntInterface sourcePropNodes, WeakRefKey<PropModelType> propItemSetId)
            : this(sourcePropNodes.GetPropNodes(), propItemSetId, sourcePropNodes.MaxPropsPerObject)
        {
        }

        private static WeakRefKey<PropModelType> CheckPropItemSetId(WeakRefKey<PropModelType>? nullableValue)
        {
            if(nullableValue.HasValue)
            {
                return nullableValue.Value;
            }
            else
            {
                throw new ArgumentException("The PropItemSetId cannot be null when creating a Fixed PropNodeCollection.");
            }
        }


        public PropNodeCollectionFixed(IEnumerable<PropNode> propNodes, WeakRefKey<PropModelType> propItemSetId, int maxPropsPerObject)
        {
            MaxPropsPerObject = maxPropsPerObject;

            if(propItemSetId.TryGetTarget(out PropModelType target))
            {
                if(target == null)
                {
                    throw new ArgumentNullException("The PropItemSetId must refer to a non-null PropModel.");
                }
            }

            _propItemSetId = propItemSetId;

            _children = propNodes.ToArray();
            PropItemsByName = _children.ToDictionary(k => k.PropertyName, v => v);

            CheckChildCount(_children.Length);

            _hashCode = ComputeHashCode();
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckChildCount(long childCount)
        {
            if (childCount > MaxPropsPerObject)
            {
                throw new InvalidOperationException("Cannot create a PropNodeCollection with more PropNodes than the MaxPropsPerObject setting.");
            }
        }

        #endregion

        #region Public Members

        public WeakRefKey<PropModelType>? PropItemSetId => _propItemSetId;

        public int Count => _children.Length;

        public int MaxPropsPerObject { get; }

        public bool IsFixed => true;

        public void Fix()
        {
            System.Diagnostics.Debug.WriteLine("The Fix method is being called on a PropNodeCollectionFixed instance.");
            // We are always fixed: so nothing to do here.
        }

        public bool Contains(PropIdType propId)
        {
            bool result = propId >= 0 && propId < _children.Length;
            return result;
        }

        public bool Contains(PropNameType propertyName)
        {
            bool result = PropItemsByName.ContainsKey(propertyName);
            return result;
        }

        public bool Contains(PropNode propNode)
        {
            foreach(PropNode x in _children)
            {
                if (x == propNode)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPropNode(PropIdType propId, out PropNode propNode)
        {
            lock (_sync)
            {
                if (Contains(propId))
                {
                    propNode = _children[propId];
                    return true;
                }
            }

            propNode = null;
            return false;
        }

        public bool TryGetPropNode(PropNameType propertyName, out PropNode propNode)
        {
            if (PropItemsByName.TryGetValue(propertyName, out propNode))
            {
                return true;
            }
            else
            {
                propNode = null;
                return false;
            }
        }

        public bool TryGetPropertyName(PropIdType propId, out PropNameType propertyName)
        {
            if(TryGetPropNode(propId, out PropNode propNode))
            {
                propertyName = propNode.PropertyName;
                return true;
            }
            else
            {
                propertyName = null;
                return false;
            }
        }

        public bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            if (PropItemsByName.TryGetValue(propertyName, out PropNode propNode))
            {
                propId = propNode.PropId;
                return true;
            }
            else
            {
                propId = PropIdType.MaxValue;
                return false;
            }
        }

        public PropNode CreateAndAdd(IPropDataInternal propData_Internal, PropNameType propertyName, BagNode parent)
        {
            throw new InvalidOperationException("Cannot Add PropItems to a Fixed PropItemSet.");
        }

        public void Add(PropNode propNode)
        {
            throw new InvalidOperationException("Cannot Add PropItems to a Fixed PropItemSet.");
        }

        public void Clear()
        {
            throw new InvalidOperationException("A fixed PropItemSet cannot be cleared.");
        }

        public bool TryRemove(PropIdType propId, out PropNode propNode)
        {
            throw new InvalidOperationException("PropItems cannot be remvoed from a fixed PropItemSet.");
        }

        #endregion

        #region Collection Support

        public IEnumerable<PropIdType> GetPropIds()
        {
            IEnumerable<PropIdType> result = _children.Select((x, idx) => (PropIdType)idx);
            return result;
        }

        public IEnumerable<PropNameType> GetPropNames()
        {
            IEnumerable<PropNameType> result = PropItemsByName.Select(x => x.Key);
            return result;
        }

        public IEnumerable<PropNode> GetPropNodes()
        {
            IEnumerable<PropNode> result = _children;
            return result;
        }

        public IEnumerable<IPropDataInternal> GetPropDataItems()
        {
            IEnumerable<IPropDataInternal> result = _children.Select(x => x.PropData_Internal);
            return result;
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetPropDataItemsWithNames()
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> result = PropItemsByName.Select(kvp => new KeyValuePair<PropNameType, IPropData>(kvp.Key, kvp.Value.PropData_Internal));
            return result;
        }

        public IReadOnlyDictionary<PropNameType, IPropData> GetPropDataItemsDict()
        {
            ReadOnlyDictionary<PropNameType, IPropData> result = 
                new ReadOnlyDictionary<PropNameType, IPropData>(PropItemsByName.ToDictionary(x => x.Key, x => (IPropData) x.Value.PropData_Internal));

            return result;
        }

        #endregion

        #region Object Overrides and IEquatable Support

        public override string ToString()
        {
            return $"Fixed PropItemSet with {Count} items.";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropNodeCollectionIntInterface);
        }

        public bool Equals(PropNodeCollectionIntInterface other)
        {
            return other != null &&
                IsFixed == other.IsFixed &&
                EqualityComparer<IReadOnlyDictionary<PropNameType, IPropData>>.Default.Equals
                    (GetPropDataItemsDict(), other.GetPropDataItemsDict());
        }

        int _hashCode;
        public override int GetHashCode() => _hashCode;

        private int ComputeHashCode()
        {
            var hashCode = 1667222605;
            hashCode = hashCode * -1521134295 + EqualityComparer<PropNode[]>.Default.GetHashCode(_children);
            hashCode = hashCode * -1521134295 + IsFixed.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PropNodeCollectionFixed collection1, PropNodeCollectionFixed collection2)
        {
            return EqualityComparer<PropNodeCollectionFixed>.Default.Equals(collection1, collection2);
        }

        public static bool operator !=(PropNodeCollectionFixed collection1, PropNodeCollectionFixed collection2)
        {
            return !(collection1 == collection2);
        }

        #endregion

        #region Private Methods

        private Dictionary<PropNameType, PropNode> BuildByNameDict(PropNode[] propNodes)
        {
            Dictionary<PropNameType, PropNode> result = propNodes.ToDictionary(k => k.PropertyName, v => v);
            return result;
        }

        #endregion
    }

}
