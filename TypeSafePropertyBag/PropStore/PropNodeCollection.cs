using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;

    internal class PropNodeCollection : PropNodeCollectionIntInterface/*, IEquatable<PropItemSetInternalInterface>*/
    {
        #region Private Members

        readonly Dictionary<PropIdType, PropNode> _children;

        Dictionary<PropNameType, PropNode> _propItemsByName;
        Dictionary<PropNameType, PropNode> PropItemsByName
        {
            get
            {
                if (_propItemsByName == null)
                {
                    lock (_sync)
                    {
                        if (_propItemsByName == null)
                        {
                            _propItemsByName = BuildByNameDict(_children);
                        }
                    }
                }

                return _propItemsByName;
            }
        }

        object _sync = new object();

        #endregion

        #region Constructor

        public PropNodeCollection(int maxPropsPerObject)
            : this(null, maxPropsPerObject)
        {
        }

        public PropNodeCollection(PropNodeCollectionIntInterface sourcePropNodes)
            : this(sourcePropNodes.GetPropNodes(), sourcePropNodes.MaxPropsPerObject)
        {
        }

        public PropNodeCollection(IEnumerable<PropNode> propNodes, int maxPropsPerObject) 
        {
            MaxPropsPerObject = maxPropsPerObject;
            if(propNodes == null)
            {
                _children = new Dictionary<PropIdType, PropNode>();
            }
            else
            {
                _children = propNodes.ToDictionary(k => k.PropId, v => v);
                CheckChildCount(_children.Count);
            }

            _propItemsByName = null;
            _isFixed = false;
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

        public int Count => _children.Count;

        public int MaxPropsPerObject { get; }

        private bool _isFixed;
        public bool IsFixed
        {
            get
            {
                return _isFixed;
            }
        }

        public void Fix()
        {
            lock(_sync)
            {
                _hashCode = ComputeHashCode();
                _isFixed = true;
            }
        }

        public bool Contains(PropIdType propId)
        {
            bool result = _children.ContainsKey(propId);
            return result;
        }

        public bool Contains(PropNameType propertyName)
        {
            bool result = _propItemsByName.ContainsKey(propertyName);
            return result;
        }

        public bool Contains(PropNode propNode)
        {
            bool result = _children.ContainsValue(propNode);
            return result;
        }

        public bool TryGetPropNode(PropIdType propId, out PropNode propNode)
        {
            if (_children.TryGetValue(propId, out propNode))
            {
                return true;
            }
            else
            {
                propNode = null;
                return false;
            }
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
            if (_children.TryGetValue(propId, out PropNode propNode))
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
            if (_isFixed)
            {
                throw new InvalidOperationException("Cannot Add PropItems to a Fixed PropItemSet.");
            }

            PropIdType nextPropId = GetNextPropId();
            PropNode newPropNode = new PropNode(nextPropId, propData_Internal, parent);
            Add(newPropNode);
            return newPropNode;
        }

        public void Add(PropNode propNode)
        {
            lock(_sync)
            {
                if(_isFixed)
                {
                    throw new InvalidOperationException("Cannot Add PropItems to a Fixed PropItemSet.");
                }

                _children.Add(propNode.PropId, propNode);

                if (_propItemsByName != null)
                {
                    _propItemsByName.Add(propNode.PropertyName, propNode);
                }
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                if(_isFixed)
                {
                    throw new InvalidOperationException("A fixed PropItemSet cannot be cleared.");
                }

                _children.Clear();
                if (_propItemsByName != null)
                {
                    _propItemsByName.Clear();
                }
            }
        }

        public bool TryRemove(PropIdType propId, out PropNode propNode)
        {
            lock (_sync)
            {
                if (_isFixed)
                {
                    throw new InvalidOperationException("PropItems cannot be remvoed from a fixed PropItemSet.");
                }

                if (_children.TryGetValue(propId, out propNode))
                {
                    _children.Remove(propId);
                    if (_propItemsByName != null)
                    {
                        _propItemsByName.Remove(propNode.PropertyName);
                    }
                    return true;
                }
            }

            propNode = null;
            return false;
        }

        #endregion

        #region Collection Support

        public IEnumerable<PropIdType> GetPropIds()
        {
            IEnumerable<PropIdType> result = _children.Select(x => x.Key);
            return result;
        }

        public IEnumerable<PropNameType> GetPropNames()
        {
            IEnumerable<PropNameType> result = PropItemsByName.Select(x => x.Key);
            return result;
        }

        public IEnumerable<PropNode> GetPropNodes()
        {
            IEnumerable<PropNode> result = _children.Select(x => x.Value);
            return result;
        }

        public IEnumerable<IPropDataInternal> GetPropDataItems()
        {
            IEnumerable<IPropDataInternal> result = _children.Select(x => x.Value.PropData_Internal);
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
            string @fixed = IsFixed ? "Fixed" : "Open";

            return $"{@fixed} PropItemSet with {Count} items.";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropNodeCollectionIntInterface);
        }

        public bool Equals(PropNodeCollectionIntInterface other)
        {
            return other != null &&
                _isFixed == other.IsFixed &&
                EqualityComparer<IReadOnlyDictionary<PropNameType, IPropData>>.Default.Equals
                    (GetPropDataItemsDict(), other.GetPropDataItemsDict());
        }

        int _hashCode;
        public override int GetHashCode()
        {
            int result;
            if (IsFixed)
            {
                result = _hashCode;
            }
            else
            {
                result = ComputeHashCode();
            }

            return result;
        }

        private int ComputeHashCode()
        {
            var hashCode = 1667222605;
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<uint, PropNode>>.Default.GetHashCode(_children);
            hashCode = hashCode * -1521134295 + _isFixed.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(PropNodeCollection collection1, PropNodeCollection collection2)
        {
            return EqualityComparer<PropNodeCollection>.Default.Equals(collection1, collection2);
        }

        public static bool operator !=(PropNodeCollection collection1, PropNodeCollection collection2)
        {
            return !(collection1 == collection2);
        }

        #endregion

        #region Private Methods

        private Dictionary<PropNameType, PropNode> BuildByNameDict(IDictionary<PropIdType, PropNode> sourceDict)
        {
            //Dictionary<PropNameType, PropNode> result = new Dictionary<PropNameType, PropNode>();

            //foreach (KeyValuePair<PropIdType, PropNode> kvp in _children)
            //{
            //    result.Add(kvp.Value.PropertyName, kvp.Value);
            //}

            Dictionary<PropNameType, PropNode> result = sourceDict.ToDictionary(k => k.Value.PropertyName, v => v.Value);
            return result;
        }

        private long m_PropIdCounter = 0;
        private PropIdType GetNextPropId()
        {
            long temp = System.Threading.Interlocked.Increment(ref m_PropIdCounter);
            if (temp > MaxPropsPerObject) throw new InvalidOperationException("The PropNodeCollection has run out of property ids.");
            return (PropIdType)temp;
        }

        //private long m_GenerationId = 0;
        //public long GetNextGenerationId()
        //{
        //    if (m_GenerationId == long.MaxValue -  1) throw new InvalidOperationException("This PropNodeCollection has over x generations: the next GenerationId exceeds the size of a long intenger.");
        //    return System.Threading.Interlocked.Increment(ref m_GenerationId);
        //}

        #endregion
    }

    //internal class SimplePropItemSetComparer : IEqualityComparer<PropItemSetInternalInterface>
    //{
    //    public bool Equals(PropItemSetInternalInterface x, PropItemSetInternalInterface y)
    //    {
    //        if (x == null && y == null)
    //            return true;

    //        if (x == null || y == null)
    //            return false;

    //        return x.IsFixed == y.IsFixed
    //            && EqualityComparer<IReadOnlyDictionary<PropNameType, IPropData>>.Default.Equals(x.GetPropDataItemsDict(), y.GetPropDataItemsDict());
    //    }

    //    public int GetHashCode(PropItemSetInternalInterface obj)
    //    {
    //        return obj.GetHashCode();
    //    }
    //}
}
