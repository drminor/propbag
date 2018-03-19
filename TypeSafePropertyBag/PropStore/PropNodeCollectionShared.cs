using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    //using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PropNameType = String;
    
    using PropItemSetKeyType = PropItemSetKey<String>;
    using PropNodeCollectionIntInterface = IPropNodeCollection_Internal<UInt32, String>;

    using PropNodelCollectionSharedInterface = IPropNodeCollectionShared<UInt32, String>;

    internal class PropNodeCollectionShared : PropNodelCollectionSharedInterface
    {
        #region Private Members

        PropItemSetKeyType _propItemSetKey;

        readonly Dictionary<ExKeyT, PropNode> _children;

        Dictionary<PropNameType, PropIdType> _nameToIdMap;

        object _sync = new object();

        #endregion

        #region Constructor

        public PropNodeCollectionShared(PropNodeCollectionIntInterface sourcePropNodes)
            : this(sourcePropNodes.GetPropNodes(), sourcePropNodes.PropItemSetKey, sourcePropNodes.MaxPropsPerObject)
        {
        }

        public PropNodeCollectionShared(IEnumerable<PropNode> propNodes, PropItemSetKeyType propItemSetKey, int maxPropsPerObject)
        {
            MaxPropsPerObject = maxPropsPerObject;

            CheckPropItemSetId(propItemSetKey);

            _propItemSetKey = propItemSetKey;

            _children = new Dictionary<ExKeyT, PropNode>();
            Add(propNodes);

            _nameToIdMap = BuildByNameDict();

            CheckChildCount(_nameToIdMap.Count);
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

        public int Count => _children.Count;

        public int PropertyCount => _nameToIdMap.Count;

        public int MaxPropsPerObject { get; }

        public bool Contains(ExKeyT compKey)
        {
            bool result = _children.Keys.Contains(compKey);
            return result;
        }

        public bool Contains(ObjectIdType objectId)
        {
            foreach(KeyValuePair<ExKeyT, PropNode> kvp in _children)
            {
                if(kvp.Key.Level1Key == objectId)
                {
                    return true;
                }
            }

            return false;
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

        public bool TryGetPropNode(ExKeyT compKey, out PropNode propNode)
        {
            bool result;

            lock (_sync)
            {
                result = _children.TryGetValue(compKey, out propNode);
            }

            return result;
        }

        public bool TryGetPropNodeCollection(ObjectIdType objectId, out PropNodeCollectionIntInterface propNodeCollection)
        {
            IEnumerable<PropNode> family = _children.Where(kvp => kvp.Key.Level1Key == objectId).Select(x => x.Value);

            if(family.FirstOrDefault() != null)
            {
                propNodeCollection = new PropNodeCollectionFixed(family, _propItemSetKey, MaxPropsPerObject);
                return true;
            }
            else
            {
                propNodeCollection = null;
                return false;
            }
        }

        public void Add(PropNodeCollectionIntInterface sourcePropNodes)
        {
            Add(sourcePropNodes.GetPropNodes());
        }

        private void Add(IEnumerable<PropNode> propNodes)
        {
            foreach(PropNode pn in propNodes)
            {
                _children.Add(pn.CompKey, pn);
            }
        }

        public bool TryRemove(PropNodeCollectionIntInterface sourcePropNodes)
        {
            throw new NotImplementedException();
        }

        public bool TryRemove(IEnumerable<ExKeyT> compKeys)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Collection Support

        public IEnumerable<PropIdType> GetPropIds()
        {
            IEnumerable<PropIdType> result = _nameToIdMap.Select((x, idx) => (PropIdType)idx);
            return result;
        }

        public IEnumerable<PropNameType> GetPropNames()
        {
            IEnumerable<PropNameType> result = _nameToIdMap.Keys;
            return result;
        }

        public IEnumerable<PropNode> GetPropNodes()
        {
            IEnumerable<PropNode> result = _children.Values;
            return result;
        }

        public IEnumerable<IPropDataInternal> GetPropDataItems()
        {
            IEnumerable<IPropDataInternal> result = _children.Values.Select(x => x.PropData_Internal);
            return result;
        }

        //public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetPropDataItemsWithNames()
        //{
        //    IEnumerable<KeyValuePair<PropNameType, IPropData>> result = PropItemsByName.Select(kvp => new KeyValuePair<PropNameType, IPropData>(kvp.Key, kvp.Value.PropData_Internal));
        //    return result;
        //}

        //public IReadOnlyDictionary<PropNameType, IPropData> GetPropDataItemsDict()
        //{
        //    ReadOnlyDictionary<PropNameType, IPropData> result = 
        //        new ReadOnlyDictionary<PropNameType, IPropData>(PropItemsByName.ToDictionary(x => x.Key, x => (IPropData) x.Value.PropData_Internal));

        //    return result;
        //}

        #endregion

        #region General Property Id And Name Support

        public bool DoesPropExist(PropIdType propId)
        {
            bool result = propId >= 0 && propId < PropertyCount;
            return result;
        }

        public bool DoesPropExist(PropNameType propertyName)
        {
            bool result = _nameToIdMap.ContainsKey(propertyName);
            return result;
        }

        public bool TryGetPropertyName(PropIdType propId, out PropNameType propertyName)
        {
            propertyName = _nameToIdMap.FirstOrDefault(kvp => kvp.Value == propId).Key;
            return propertyName != null;
        }

        public bool TryGetPropId(PropNameType propertyName, out PropIdType propId)
        {
            if (_nameToIdMap.TryGetValue(propertyName, out propId))
            {
                return true;
            }
            else
            {
                propId = PropIdType.MaxValue;
                return false;
            }
        }

        #endregion

        #region Object Overrides and IEquatable Support

        public override string ToString()
        {
            return $"Shared Prop Collection: {_propItemSetKey.FullClassName}, {_propItemSetKey.GenerationId}";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Assumes that propNodes contains a complete set of child PropNodes for a particular BagNode, and contains no other PropNodes.
        /// </summary>
        /// <param name="propNodes"></param>
        /// <returns></returns>
        private Dictionary<PropNameType, PropIdType> BuildByNameDict(IEnumerable<PropNode> propNodes)
        {
            Dictionary<PropNameType, PropIdType> result = propNodes.ToDictionary(k => k.PropertyName, v => v.PropId);
            return result;
        }

        /// <summary>
        /// Assumes that propNodes contains a complete set of child PropNodes for a particular BagNode and may or may not contain
        /// propNodes from other BagNodes. (Sharing the same PropItemSetKey, of course.)
        /// </summary>
        /// <returns></returns>
        private Dictionary<PropNameType, PropIdType> BuildByNameDict()
        {
            ObjectIdType objectId = _children.Keys.First()?.Level1Key ?? (UInt64)0;

            var repFam = _children.Where(x => x.Key.Level1Key == objectId);

            Dictionary<PropNameType, PropIdType> result = repFam.ToDictionary(k => k.Value.PropertyName, v => v.Value.PropId);
            return result;
        }

        #endregion
    }
}
