﻿using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;

    using PropIdType = UInt32;
    using PropNameType = String;

    public sealed class SimplePropStoreAccessService
        : IPropStoreAccessService<PropIdType, PropNameType>, IHaveTheSimpleKey
    {
        #region Private Members

        WeakReference<IPropBag> _clientAccessToken;
        ObjectIdType _objectId;

        SimpleCompKeyMan _compKeyManager;
        SimpleLevel2KeyMan _level2KeyManager;

        SimpleObjectIdDictionary _theGlobalStore;
        //IReadOnlyDictionary<ObjectRefType, ObjectIdType> _objectIdDictionary;

        #endregion

        #region Constructor

        public SimplePropStoreAccessService
            (
            WeakReference<IPropBag> clientAccessToken,
            ObjectIdType objectId,
            SimpleObjectIdDictionary theGlobalStore,
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

        public IPropGen this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                SimpleExKey exKey = GetExKey(propBag, propId);
                IPropGen result = _theGlobalStore[exKey];

                return result;
            }
            set
            {
                SimpleExKey exKey = GetExKey(propBag, propId);
                _theGlobalStore[exKey] = value;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropGen propData)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.TryGetValue(exKey, out propData);
            return result;
        }

        public bool TryAdd(IPropBag propBag, PropIdType propId, IPropGen propData)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.TryAdd(exKey, propData);
            return result;
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropGen propData)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

            bool result = _theGlobalStore.TryRemove(exKey, out propData);
            return result;
        }
         
        public bool ContainsKey(IPropBag propBag, PropIdType propId)
        {
            SimpleExKey exKey = GetExKey(propBag, propId);

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

        SimpleExKey GetExKey(IPropBag propBag, PropIdType propId)
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
            SimpleExKey result = GetExKey(propBag, propId);
            return result;
        }

        IExplodedKey<CompositeKeyType, ObjectIdType, PropIdType> IHaveTheKey<CompositeKeyType, ObjectIdType, PropIdType>.GetTheKey(IPropBag propBag, PropIdType propId)
        {
            IExplodedKey<CompositeKeyType, ObjectIdType, PropIdType> result = GetExKey(propBag, propId);
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
