using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using ObjectRefType = WeakReference<IPropBag>;

    using PropIdType = UInt32;
    using PropNameType = String;

    public sealed class SimplePropStoreAccessService<PropBagT, PropDataT>
        : IPropStoreAccessService<PropBagT, PropDataT>, IHaveTheKey<PropBagT>
        where PropBagT : IPropBag
        where PropDataT : IPropGen
    {
        #region Private Members

        ObjectRefType _clientAccessToken;
        ObjectIdType _objectId;

        SimpleCompKeyMan _compKeyManager;
        SimpleLevel2KeyMan _level2KeyManager;

        SimpleObjectIdDictionary<PropDataT> _theGlobalStore;
        //IReadOnlyDictionary<ObjectRefType, ObjectIdType> _objectIdDictionary;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            ObjectRefType clientAccessToken,
            ObjectIdType objectId,
            SimpleObjectIdDictionary<PropDataT> theGlobalStore,
            SimpleCompKeyMan compKeyManager,
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

        public PropDataT this[PropBagT propBag, PropIdType propId]
        {
            get
            {
                SimpleExKey exKey = GetExKey(propBag, propId);
                PropDataT result = _theGlobalStore[exKey];

                return result;
            }
            set
            {
                SimpleExKey exKey = GetExKey(propBag, propId);
                _theGlobalStore[exKey] = value;
            }
        }

        public bool TryGetValue(PropBagT propBag, PropIdType propId, out PropDataT propData)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.TryGetValue(exKey, out propData);
            return result;
        }

        public bool TryAdd(PropBagT propBag, PropIdType propId, PropDataT propData)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.TryAdd(exKey, propData);
            return result;
        }

        public bool TryRemove(PropBagT propBag, PropIdType propId, out PropDataT propData)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.TryRemove(exKey, out propData);
            return result;
        }
         
        public bool ContainsKey(PropBagT propBag, PropIdType propId)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.ContainsKey(exKey);
            return result;
        }

        // TODO: Consider keeping a index of all Props,
        // or making the caller remove each Prop individually.
        public void Clear(PropBagT propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, PropDataT>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);
            foreach(KeyValuePair<SimpleExKey, PropDataT> kvp in propDataObjectsForThisPropBag)
            {
                _theGlobalStore.TryRemove(kvp.Key, out PropDataT dontNeedItVal);
            }
        }

        public IEnumerable<KeyValuePair<PropNameType, PropDataT>> GetCollection(PropBagT propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, PropDataT>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);

            IEnumerable<KeyValuePair<PropNameType, PropDataT>> result = propDataObjectsForThisPropBag.Select(x =>
                new KeyValuePair<PropNameType, PropDataT>(GetPropNameFromL2Key(x.Key.Level2Key), x.Value));

            return result;
        }

        public IEnumerator<KeyValuePair<PropNameType, PropDataT>> GetEnumerator(PropBagT propBag) 
        {
            IEnumerable<KeyValuePair<PropNameType, PropDataT>> list = GetCollection(propBag);
            IEnumerator<KeyValuePair<PropNameType, PropDataT>> result = list.GetEnumerator();
            return result;
        }

        public IEnumerable<PropNameType> GetKeys(PropBagT propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, PropDataT>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);

            IEnumerable<PropNameType> result = propDataObjectsForThisPropBag.Select(x =>
                GetPropNameFromL2Key(x.Key.Level2Key));

            return result;
        }

        public IEnumerable<PropDataT> GetValues(PropBagT propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<SimpleExKey, PropDataT>> propDataObjectsForThisPropBag = _theGlobalStore.Where(x => x.Key.Level1Key == objectId);

            IEnumerable<PropDataT> result = propDataObjectsForThisPropBag.Select(x => x.Value);

            return result;
        }

        private string GetPropNameFromL2Key(PropIdType l2Key)
        {
            string propertyName = _level2KeyManager.FromCooked(l2Key);
            return propertyName;
        }

        #endregion

        #region Private Methods

        SimpleExKey GetExKey(PropBagT propBag, PropIdType propId)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);
            CompositeKeyType cKey = _compKeyManager.JoinComp(objectId, propId);
            SimpleExKey exKey = new SimpleExKey(cKey, _clientAccessToken, objectId, propId);
            return exKey;
        }

        ObjectIdType GetAndCheckObjectRef(PropBagT propBag)
        {
            PropBagT client = UnwrapWeakRef(_clientAccessToken);
            if (!object.ReferenceEquals(propBag, client))
            {
                throw new InvalidOperationException("This PropStoreAccessService can only service the PropBag object that created it.");
            }

            ObjectIdType result = _objectId;

            return result;
        }

        private PropBagT UnwrapWeakRef(ObjectRefType oRef)
        {
            if (oRef.TryGetTarget(out IPropBag target))
            {
                PropBagT result = (PropBagT)target;
                return result;
            }
            else
            {
                return default(PropBagT);
            }
        }

        SimpleExKey IHaveTheKey<PropBagT>.GetTheKey(PropBagT propBag, uint propId)
        {
            SimpleExKey result = GetExKey(propBag, propId);
            return result;
        }

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
