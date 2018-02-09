using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    internal class PropNodeCollection : IPropNodeCollection
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
        {
            MaxPropsPerObject = maxPropsPerObject;
            _children = new Dictionary<PropIdType, PropNode>();
            _propItemsByName = null;
        }

        public PropNodeCollection(IPropNodeCollection sourcePropNodes, BagNode targetParent)
        {
            MaxPropsPerObject = sourcePropNodes.MaxPropsPerObject;

            _children = new Dictionary<PropIdType, PropNode>();

            foreach(PropNode propNode in sourcePropNodes.GetPropNodes())
            {
                IProp newTypeProp = (IProp) propNode.PropData_Internal.TypedProp.Clone();

                IPropDataInternal newPropData_Internal = new PropGen(newTypeProp);

                PropNode newPropNode = new PropNode(propNode.PropId, newPropData_Internal, targetParent);
                Add(newPropNode);
            }

            _propItemsByName = null;
        }

        #endregion

        #region Public Members

        public int Count => _children.Count;

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
                    _propItemsByName.Add(propNode.PropData_Internal.TypedProp.PropertyName, propNode);
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
                        _propItemsByName.Remove(propNode.PropData_Internal.TypedProp.PropertyName);
                    }
                    return true;
                }
            }

            propNode = null;
            return false;
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
        private PropIdType GetNextPropId()
        {
            long temp = System.Threading.Interlocked.Increment(ref m_Counter);
            if (temp > MaxPropsPerObject) throw new InvalidOperationException("The SimpleLevel2Key Manager has run out of property ids.");
            return (PropIdType)temp;
        }

        #endregion
    }
}
