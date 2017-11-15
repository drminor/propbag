using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt32, UInt32>;
    using IHaveTheKeyIT = IHaveTheKey<UInt64, UInt32, UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    using ICKeyManType = ICKeyMan<UInt64, UInt32, UInt32, String>;
    using L2KeyManType = IL2KeyMan<UInt32, String>;

    public sealed class SimplePropStoreAccessService
        : PSAccessServiceType, IHaveTheSimpleKey
    {
        #region Private Members

        SimpleExKey _parentPropDataRef; // If this propBag is being managed by a "parenting" IPropData object on some other IPropBag, this will hold a reference to that IPropData object.

        WeakReference<IPropBag> _clientAccessToken;
        ObjectIdType _objectId;

        ICKeyManType _compKeyManager;
        L2KeyManType _level2KeyManager;

        SimpleObjectIdDictionary _theGlobalStore;
        //IReadOnlyDictionary<ObjectRefType, ObjectIdType> _objectIdDictionary;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            WeakReference<IPropBag> clientAccessToken,
            ObjectIdType objectId,
            SimpleObjectIdDictionary theGlobalStore,
            ICKeyManType compKeyManager,
            L2KeyManType level2KeyManager
            )
        {
            _parentPropDataRef = new SimpleExKey();
            _clientAccessToken = clientAccessToken;
            _objectId = objectId;
            _theGlobalStore = theGlobalStore;
            _compKeyManager = compKeyManager;
            _level2KeyManager = level2KeyManager;

            MaxPropsPerObject = compKeyManager.MaxPropsPerObject;
            MaxObjectsPerAppDomain = compKeyManager.MaxObjectsPerAppDomain;
        }

        #endregion

        #region Member We Are Working On

        //public object GetVisa(IPropBag requstingPropBag, PropIdType propId, IPropBag dataSource, string sourcePath)
        //{
        //    return new object();
        //}

        public ObjectIdType GetParentObjectId(IPropBag propBag)
        {
            GetAndCheckObjectRef(propBag);
            return _parentPropDataRef.Level1Key;
        }

        public bool SetChildObjectId(IPropBag propBag, PropIdType propId, ObjectIdType childObjectId)
        {
            CompositeKeyType cKey = GetCompKey(propBag, propId);

            if(_theGlobalStore.TryGetValue(cKey, out IPropData propData))
            {
                ((IPropDataInternal)propData).SetChildObjectId(childObjectId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetTypedProp(IPropBag propBag, PropIdType propId, IProp genericTypedProp)
        {
            CompositeKeyType exKey = GetCompKey(propBag, propId);

            if (_theGlobalStore.TryGetValue(exKey, out IPropData propData))
            {
                ((IPropDataInternal)propData).SetTypedProp(genericTypedProp);
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; }

        public IPropData this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                CompositeKeyType exKey = GetCompKey(propBag, propId);
                IPropData result = _theGlobalStore[exKey];

                return result;
            }
            private set
            {
                CompositeKeyType exKey = GetCompKey(propBag, propId);
                _theGlobalStore[exKey] = value;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            CompositeKeyType cKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryGetValue(cKey, out propData);
            return result;
        }

        public bool TryAdd(IPropBag propBag, PropIdType propId, IPropData propData)
        {
            if(propData.IsPropBag)
            {
                SimpleExKey exKey = GetExKey(propBag, propId);
                bool result = _theGlobalStore.TryAdd(exKey.CKey, propData);

                // Set the ParentKey of the child propData item to reference the global PropId of this new property.
                // Cast the payload of the Prop Data item to IPropBag
                object test =  propData.TypedProp.TypedValueAsObject;

                ObjectIdType childPropBagObjectId = 0;
                if (propData?.TypedProp?.TypedValueAsObject is IPropBag child)
                {
                    // Get a reference to the Child's StoreAccessor's IHaveTheSimpleKey interface.
                    PSAccessServiceType StoreAccessForChild = child.OurStoreAccessor;
                    // Set the ParentKey on that StoreAccess to the GlobalKey for the PropData item that is being added.

                    if (StoreAccessForChild is IHaveTheSimpleKey keyAccess)
                    {
                        keyAccess.ParentKey = exKey;
                        childPropBagObjectId = keyAccess.ObjectId;
                    }
                }

                if (propData is IPropDataInternal pgi)
                {
                    pgi.SetCompKey(exKey.CKey);
                    pgi.SetChildObjectId(childPropBagObjectId);

                    propData.PropertyChangedWithObjectVals += PropData_PropertyChangedWithObjectVals; 
                }

                return result;
            } 
            else
            {
                CompositeKeyType cKey = GetCompKey(propBag, propId);
                bool result = _theGlobalStore.TryAdd(cKey, propData);

                if (propData is IPropDataInternal pgi)
                {
                    pgi.SetCompKey(cKey);
                }

                return result;
            }
        }

        private void PropData_PropertyChangedWithObjectVals(object sender, PCObjectEventArgs e)
        {
            if(e.PropertyName == "TypedProp")
            {
                if(sender is IPropData pd)
                {
                    if(pd?.TypedProp?.TypedValueAsObject is IPropBag child)
                    {
                        if(pd.ChildObjectId != 0)
                        {
                            // Get a reference to the Child's StoreAccessor's IHaveTheSimpleKey interface.
                            PSAccessServiceType StoreAccessForChild = child.OurStoreAccessor;
                            // Set the ParentKey on that StoreAccess to the GlobalKey for the PropData item that is being added.

                            if (StoreAccessForChild is IHaveTheSimpleKey keyAccess)
                            {
                                // TODO: Fix Me.
                                //keyAccess.ParentKey = pd.ChildObjectId;
                            }
                        }

                    }
                }
            }
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropData propData)
        {
            CompositeKeyType exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryRemove(exKey, out propData);
            return result;
        }
         
        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            CompositeKeyType exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.ContainsKey(exKey);
            return result;
        }

        // TODO: Consider keeping a index of all Props,
        // or making the caller remove each Prop individually.
        public void Clear(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<CompositeKeyType, IPropData>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);
            foreach(KeyValuePair<CompositeKeyType, IPropData> kvp in propDataObjectsForThisPropBag)
            {
                _theGlobalStore.TryRemove(kvp.Key, out IPropData dontNeedItVal);
            }
        }

        // TODO: Consider adding a method that returns IEnumerable<KeyValuePair<PropIdType, IPropData>>
        public IEnumerable<KeyValuePair<PropNameType, IPropData>> GetCollection(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            //IEnumerable<KeyValuePair<CompositeKeyType, IPropData>> propDataObjectsForThisPropBag =

            List<KeyValuePair<CompositeKeyType,IPropData>> propDataObjectsForThisPropBag = 
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId).ToList();

            Dictionary<PropNameType, IPropData> result = new Dictionary<PropNameType, IPropData>();
            foreach (KeyValuePair<CompositeKeyType, IPropData> kvp in propDataObjectsForThisPropBag)
            {
                string s = GetPropNameFromKey(kvp.Key);
                result.Add(s, kvp.Value);
            }

            //IEnumerable<KeyValuePair<PropNameType, IPropData>> result = propDataObjectsForThisPropBag.Select(x =>
            //    new KeyValuePair<PropNameType, IPropData>(GetPropNameFromKey(x.Key), x.Value));

            return result;
        }

        public IEnumerator<KeyValuePair<PropNameType, IPropData>> GetEnumerator(IPropBag propBag) 
        {
            IEnumerable<KeyValuePair<PropNameType, IPropData>> list = GetCollection(propBag);
            IEnumerator<KeyValuePair<PropNameType, IPropData>> result = list.GetEnumerator();
            return result;
        }

        public IEnumerable<PropNameType> GetKeys(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<CompositeKeyType, IPropData>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);

            IEnumerable<PropNameType> result = propDataObjectsForThisPropBag.Select(x =>
                GetPropNameFromKey(x.Key));

            return result;
        }

        public IEnumerable<IPropData> GetValues(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<CompositeKeyType, IPropData>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);

            IEnumerable<IPropData> result = propDataObjectsForThisPropBag.Select(x => x.Value);

            return result;
        }

        #endregion

        #region Private Methods

        CompositeKeyType GetCompKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            CompositeKeyType cKey = _compKeyManager.JoinComp(objectId, propId);

            return cKey;
        }

        private string GetPropNameFromKey(CompositeKeyType cKey)
        {
            //ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropNameType propertyName);

            ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropIdType propId);

            if(_level2KeyManager.TryGetFromCooked(propId, out PropNameType propertyName))
            {
                return propertyName;
            }
            else
            {
                throw new KeyNotFoundException($"The cKey: {cKey}, which included PropId: {propId} does not correspond with any registered propertyName.");
            }

        }

        private ObjectIdType GetObjectIdFromKey(CompositeKeyType cKey)
        {
            ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropIdType propId);
            return objectId;
        }

        private SimpleExKey GetExKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            CompositeKeyType cKey = _compKeyManager.JoinComp(objectId, propId);
            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, objectId, propId);
            return exKey;
        }

        private SimpleExKey GetExKey(IPropBag propBag, CompositeKeyType cKey)
        {
            ObjectIdType objectIdFromCKey = _compKeyManager.SplitComp(cKey, out PropIdType propId);
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            if(!object.ReferenceEquals(objectId, objectIdFromCKey))
            {
                throw new InvalidOperationException("The composite key given does not match the value given for propBag.");

            }

            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, objectId, propId);
            return exKey;
        }

        // Also verifies that the compKey matches the propBag value.
        // Don't use this if you need an ExKey, since 1) ExKey requires a PropId value, 2) to get a PropId value you need split the CompKey, 3) Verify Splits the CompKey, 4) you can easily verify by using reference equality to see that our _objectId == the objectId from the CompKey.
        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag, CompositeKeyType cKey)
        {
            ObjectIdType result = GetAndCheckObjectRef(propBag);
            if(!_compKeyManager.Verify(cKey, result))
            {
                throw new InvalidOperationException("The composite key given does not match the value given for propBag.");
            }

            return result;
        }

        private ObjectIdType GetAndCheckObjectRef(IPropBag propBag)
        {
            IPropBag client = SimpleExKey.UnwrapWeakRef(_clientAccessToken);
            if (client == null)
            {
                throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it. The weak reference given holds no object.");
            }
            if (!object.ReferenceEquals(propBag, client))
            {
                throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
            }

            ObjectIdType result = _objectId;

            return result;
        }

        #endregion

        #region Explicit Implementation of the internal interface: IHaveTheKey

        public ObjectIdType ObjectId => _objectId;

        SimpleExKey IHaveTheSimpleKey.GetTheKey(IPropBag propBag, PropIdType propId)
        {
            SimpleExKey result = GetExKey(propBag, propId);
            return result;
        }

        ExKeyT IHaveTheKeyIT.GetTheKey(IPropBag propBag, PropIdType propId)
        {
            ExKeyT result = ((IHaveTheSimpleKey)this).GetTheKey(propBag, propId);
            return result;
        }

        SimpleExKey IHaveTheSimpleKey.ParentKey
        {
            get
            {
                return _parentPropDataRef;
            }
            set
            {
                _parentPropDataRef = value;
            }
        }

        ExKeyT IHaveTheKeyIT.ParentKey
        {
            get
            {
                return _parentPropDataRef;
            }
            set
            {
                _parentPropDataRef = (SimpleExKey)value;
            }
        }

        #endregion

    }
}
