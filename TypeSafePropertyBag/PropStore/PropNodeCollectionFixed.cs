﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using PropItemSetKeyType = PropItemSetKey<String>;
    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;

    internal class PropNodeCollectionFixed : PropNodeCollectionIntInterface/*, IEquatable<PropItemSetInternalInterface>*/
    {
        #region Private Members

        PropItemSetKeyType _propItemSetKey;
        PropNode[] _children;
        Dictionary<PropNameType, PropNode> PropItemsByName;
        object _sync = new object();

        #endregion

        #region Constructor

        public PropNodeCollectionFixed(PropNodeCollectionIntInterface sourcePropNodes, PropItemSetKeyType propItemSetKey)
            : this(sourcePropNodes.GetPropNodes(), propItemSetKey, sourcePropNodes.MaxPropsPerObject)
        {
        }

        public PropNodeCollectionFixed(IEnumerable<PropNode> propNodes, PropItemSetKeyType propItemSetKey, int maxPropsPerObject)
        {
            MaxPropsPerObject = maxPropsPerObject;

            CheckPropItemSetId(propItemSetKey);
            _propItemSetKey = propItemSetKey;

            _children = PopulateChildren(propNodes);
            PropItemsByName = _children.ToDictionary(k => k.PropertyName, v => v);
            _hashCode = ComputeHashCode();
        }

        private PropNode[] PopulateChildren(IEnumerable<PropNode> propNodes)
        {
            long maxPropId = propNodes.Max(pn => pn.PropId);
            CheckChildCount(maxPropId - 1);

            PropNode[] result = new PropNode[maxPropId];

            foreach(PropNode pn in propNodes)
            {
                result[pn.PropId - 1] = pn; // PropIds start at 1, our array's first index is 0.
            }

            return result;
        }

        private void CheckPropItemSetId(PropItemSetKeyType propItemSetKey)
        {
            if (propItemSetKey.IsEmpty)
            {
                throw new ArgumentException("The PropItemSetKey cannot be empty when creating a Fixed PropNodeCollection.");
            }
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
            bool result = propId > 0 && propId <= _children.Length;
            return result;
        }

        public bool Contains(PropNameType propertyName)
        {
            bool result = PropItemsByName.ContainsKey(propertyName);
            return result;
        }

        public bool Contains(PropNode propNode)
        {
            foreach (PropNode x in _children)
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
            lock (_sync)
            {
                if (Contains(propId))
                {
                    propNode = _children[propId - 1]; // Our Array begin at index 0; PropIds start at 1. (0 is reserved for none or all Properties.)
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

        #endregion

        #region Invalid Methods

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
            throw new InvalidOperationException("PropItems cannot be removed from a fixed PropItemSet.");
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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //Dispose managed state (managed objects).
                    PropItemsByName.Clear();
                    PropItemsByName = null;
                    _children = null;
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
