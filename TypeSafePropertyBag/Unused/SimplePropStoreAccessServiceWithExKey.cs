using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyType = IExplodedKey<UInt64, UInt32, UInt32>;
    //using HaveTheKeyType = IHaveTheKey<UInt64, UInt32, UInt32>;

    public sealed class SimplePropStoreAccessServiceWithExKey
        : IPropStoreAccessService<PropIdType, PropNameType>, IHaveTheStoreNode
    {
        #region Private Members

        WeakReference<IPropBag> _clientAccessToken;
        ObjectIdType _objectId;

        SimpleCompExKeyMan _compKeyManager;
        SimpleLevel2KeyMan _level2KeyManager;

        //SimpleObjectExIdDictionary _theGlobalStore;
        //IReadOnlyDictionary<ObjectRefType, ObjectIdType> _objectIdDictionary;

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceWithExKey
            (
            WeakReference<IPropBag> clientAccessToken,
            ObjectIdType objectId,
            SimpleObjectExIdDictionary theGlobalStore,
            SimpleExCompKeyMan compKeyManager,
            SimpleLevel2KeyMan level2KeyManager
            )
        {
            _clientAccessToken = clientAccessToken;
            _objectId = objectId;
            _theGlobalStore = theGlobalStore;
            _compKeyManager = compKeyManager;
            _level2KeyManager = level2KeyManager;
            MaxPropsPerObject = compKeyManager.MaxPropsPerObject;
            MaxObjectsPerAppDomain = compKeyManager.MaxObjectsPerAppDomain;
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; }

        public IPropGen this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                SimpleExKey exKey = GetCompKey(propBag, propId);
                IPropGen result = _theGlobalStore[exKey];

                return result;
            }
            set
            {
                SimpleExKey exKey = GetCompKey(propBag, propId);
                _theGlobalStore[exKey] = value;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropGen propData)
        {
            SimpleExKey exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryGetValue(exKey, out propData);
            return result;
        }

        public bool TryAdd(IPropBag propBag, PropIdType propId, IPropGen propData)
        {
            SimpleExKey exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryAdd(exKey, propData);
            return result;
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropGen propData)
        {
            SimpleExKey exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryRemove(exKey, out propData);
            return result;
        }
         
        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            SimpleExKey exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.ContainsKey(exKey);
            return result;
        }

        // TODO: Consider keeping a index of all Props,
        // or making the caller remove each Prop individually.
        public void Clear(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, IPropGen>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);
            foreach(KeyValuePair<SimpleExKey, IPropGen> kvp in propDataObjectsForThisPropBag)
            {
                _theGlobalStore.TryRemove(kvp.Key, out IPropGen dontNeedItVal);
            }
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropGen>> GetCollection(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, IPropGen>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);

            IEnumerable<KeyValuePair<PropNameType, IPropGen>> result = propDataObjectsForThisPropBag.Select(x =>
                new KeyValuePair<PropNameType, IPropGen>(GetPropNameFromL2Key(x.Key.Level2Key), x.Value));

            return result;
        }

        public IEnumerator<KeyValuePair<PropNameType, IPropGen>> GetEnumerator(IPropBag propBag) 
        {
            IEnumerable<KeyValuePair<PropNameType, IPropGen>> list = GetCollection(propBag);
            IEnumerator<KeyValuePair<PropNameType, IPropGen>> result = list.GetEnumerator();
            return result;
        }

        public IEnumerable<PropNameType> GetKeys(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, IPropGen>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);

            IEnumerable<PropNameType> result = propDataObjectsForThisPropBag.Select(x =>
                GetPropNameFromL2Key(x.Key.Level2Key));

            return result;
        }

        public IEnumerable<IPropGen> GetValues(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, IPropGen>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);

            IEnumerable<IPropGen> result = propDataObjectsForThisPropBag.Select(x => x.Value);

            return result;
        }

        private string GetPropNameFromL2Key(PropIdType l2Key)
        {
            string propertyName = _level2KeyManager.FromCooked(l2Key);
            return propertyName;
        }

        #endregion

        #region Private Methods

        SimpleExKey GetCompKey(IPropBag propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            CompositeKeyType cKey = _compKeyManager.JoinComp(objectId, propId);
            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, objectId, propId);
            return exKey;
        }

        ObjectIdType GetAndCheckObjectRef(IPropBag propBag)
        {
            IPropBag client = SimpleExKey.UnwrapWeakRef(_clientAccessToken);
            if (!object.ReferenceEquals(propBag, client))
            {
                throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
            }

            ObjectIdType result = _objectId;

            return result;
        }

        #region Explicit Implementation of the internal interface: IHaveTheKey

        SimpleExKey IHaveTheSimpleKey.GetTheKey(IPropBag propBag, PropIdType propId)
        {
            SimpleExKey result = GetCompKey(propBag, propId);
            return result;
        }

        ExKeyType HaveTheKeyType.GetTheKey(IPropBag propBag, PropIdType propId)
        {
            ExKeyType result = GetCompKey(propBag, propId);
            return result;
        }

        #endregion

        //ObjectIdType FromRaw(ObjectRefType rawBot)
        //{
        //    ObjectIdType result = _objectIdDictionary[rawBot];
        //    return result;
        //}

        //bool TryGetFromRaw(ObjectRefType rawBot, out ObjectIdType bot)
        //{
        //    if (_objectIdDictionary.TryGetValue(rawBot, out bot))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        bot = 0;
        //        return false;
        //    }
        //}

        #endregion
    }
}
