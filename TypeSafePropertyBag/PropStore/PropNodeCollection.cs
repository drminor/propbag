using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using System.Collections;
    using System.Linq;

    internal class PropNodeCollection : ICollection<KeyValuePair<PropIdType, PropNode>>
    {
        #region Private Members

        private readonly Dictionary<PropIdType, PropNode> _children;
        private Dictionary<PropNameType, PropNode> _propItemsByName;

        private object _sync = new object();

        private Dictionary<PropNameType, PropNode> PropItemsByName
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

        #endregion

        #region Constructor

        public PropNodeCollection(int maxPropsPerObject)
        {
            MaxPropsPerObject = maxPropsPerObject;
            _children = new Dictionary<PropIdType, PropNode>();
            _propItemsByName = null;
        }

        #endregion

        #region Public Members

        public int Count => _children.Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<uint, PropNode>>)_children).IsReadOnly;

        public int MaxPropsPerObject { get; }

        public bool IsFixed { get; private set; }

        public void Fix()
        {
            IsFixed = true;
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

        public PropNameType GetPropertyName(PropIdType propId)
        {
            PropNameType result = _children[propId].PropData_Internal.TypedProp.PropertyName;
            return result;
        }

        public bool TryGetPropertyName(PropIdType propId, out PropNameType propertyName)
        {
            if (_children.TryGetValue(propId, out PropNode propNode))
            {
                propertyName = propNode.PropData_Internal.TypedProp.PropertyName;
                return true;
            }
            else
            {
                propertyName = null;
                return false;
            }
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

        public PropIdType GetPropId(PropNameType propertyName)
        {
            PropIdType result = PropItemsByName[propertyName].PropId;
            return result;
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

        public PropNode Add(IPropDataInternal propData_Internal, PropNameType propertyName, BagNode propBagNode)
        {
            PropIdType nextPropId = GetNextPropId();

            ExKeyT exKey = new SimpleExKey(propBagNode.ObjectId, nextPropId);

            PropNode newPropNode = new PropNode(exKey, propData_Internal, propBagNode);
            _children.Add(nextPropId, newPropNode);

            return newPropNode;
        }

        public void Add(KeyValuePair<PropIdType, PropNode> item)
        {
            Add(item.Key, item.Value);
        }

        public void Add(PropIdType propId, PropNode propNode)
        {
            lock(_sync)
            {
                _children.Add(propId, propNode);
                if (_propItemsByName != null)
                {
                    _propItemsByName.Add(propNode.PropData_Internal.TypedProp.PropertyName, propNode);
                }
            }
        }

        public void Clear()
        {
            _children.Clear();
        }

        public bool Contains(KeyValuePair<uint, PropNode> item)
        {
            return ((ICollection<KeyValuePair<uint, PropNode>>)_children).Contains(item);
        }

        public void CopyTo(KeyValuePair<uint, PropNode>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<uint, PropNode>>)_children).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<uint, PropNode> item)
        {
            return ((ICollection<KeyValuePair<uint, PropNode>>)_children).Remove(item);
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
                        _propItemsByName.Remove(propNode.PropData_Internal.TypedProp.PropertyName);
                    }
                    return true;
                }
            }

            propNode = null;
            return false;
       }

        public IEnumerator<KeyValuePair<uint, PropNode>> GetEnumerator()
        {
            return ((ICollection<KeyValuePair<uint, PropNode>>)_children).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<uint, PropNode>>)_children).GetEnumerator();
        }

        #endregion

        #region Private Methods

        private Dictionary<PropNameType, PropNode> BuildByNameDict(IDictionary<PropIdType, PropNode> sourceDict)
        {
            Dictionary<PropNameType, PropNode> result = new Dictionary<PropNameType, PropNode>();

            foreach (KeyValuePair<PropIdType, PropNode> kvp in _children)
            {
                result.Add(kvp.Value.PropData_Internal.TypedProp.PropertyName, kvp.Value);
            }

            return result;
        }

        private long m_Counter = 0;
        private uint GetNextPropId()
        {
            long temp = System.Threading.Interlocked.Increment(ref m_Counter);
            if (temp > MaxPropsPerObject) throw new InvalidOperationException("The SimpleLevel2Key Manager has run out of property ids.");
            return (uint)temp;
        }



        #endregion
    }
}
