using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PropItemSetKeyType = PropItemSetKey<String>;
    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;

    internal class PropNodeCollection : PropNodeCollectionIntInterface/*, IEquatable<PropItemSetInternalInterface>*/
    {
        #region Private Members

        //WeakRefKey<PropModelType>? _propItemSetId;
        PropItemSetKeyType _propItemSetKey;

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

        public PropNodeCollection()
        {
            throw new InvalidOperationException("Don't use the parameterless constructor to create a PropNodeCollection.");
        }

        public PropNodeCollection(int maxPropsPerObject)
            : this(null, PropItemSetKeyType.Empty, maxPropsPerObject)
        {
        }

        public PropNodeCollection(PropNodeCollectionIntInterface sourcePropNodes)
            : this(sourcePropNodes.GetPropNodes(), sourcePropNodes.PropItemSetKey, sourcePropNodes.MaxPropsPerObject)
        {
        }

        public PropNodeCollection(IEnumerable<PropNode> propNodes, PropItemSetKeyType propItemSetKey, int maxPropsPerObject) 
        {
            MaxPropsPerObject = maxPropsPerObject;

            _propItemSetKey = propItemSetKey;

            if(propNodes == null)
            {
                _children = new Dictionary<PropIdType, PropNode>();
                m_PropIdCounter = -1;
            }
            else
            {
                _children = propNodes.ToDictionary(k => k.PropId, v => v);
                CheckChildCount(_children.Count);
                m_PropIdCounter = _children.Count - 1;
            }

            _propItemsByName = null;
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

        public PropItemSetKeyType PropItemSetKey => _propItemSetKey;

        public int Count => _children.Count;

        public int MaxPropsPerObject { get; }

        public bool IsFixed => false;

        public void Fix()
        {
            throw new InvalidOperationException("RegularPropNodeCollection instances cannot be fixed.");
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
            foreach (PropNode x in _children.Values)
            {
                if (ReferenceEquals(x, propNode))
                {
                    return true;
                }
            }

            return false;
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
            PropIdType nextPropId = GetNextPropId();
            PropNode newPropNode = new PropNode(nextPropId, propData_Internal, parent);
            Add(newPropNode);
            return newPropNode;
        }

        public void Add(PropNode propNode)
        {
            lock(_sync)
            {
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
            return $"Open PropItemSet with {Count} items.";
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

        public override int GetHashCode()
        {
            int result;
            result = ComputeHashCode();
            return result;
        }

        private int ComputeHashCode()
        {
            var hashCode = 1667222605;
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<uint, PropNode>>.Default.GetHashCode(_children);
            hashCode = hashCode * -1521134295 + IsFixed.GetHashCode();
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
            Dictionary<PropNameType, PropNode> result = sourceDict.ToDictionary(k => k.Value.PropertyName, v => v.Value);
            return result;
        }

        private long m_PropIdCounter;
        private PropIdType GetNextPropId()
        {
            long temp = System.Threading.Interlocked.Increment(ref m_PropIdCounter);
            if (temp > MaxPropsPerObject) throw new InvalidOperationException("The PropNodeCollection has run out of property ids.");
            return (PropIdType)temp;
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
