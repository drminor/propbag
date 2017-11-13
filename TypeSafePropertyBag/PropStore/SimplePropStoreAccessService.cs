﻿using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Visa Request Package:

//internal:
//targetCompKey(must match public version.)
//target's access token (WR<IPropBag>)
//target's Access Service [TAS Proxy] (WR<IPropStoreAccessService>)
//a visa request token[VRT] created by the target's access service
//(the VRT will be passsed back to the target's access
//service on each update request for inspection by the target's 
//access service.)

//Typically the TAS will create a single VRT upon construction.
//this VRT will never be placed in a field not marked as private.

//Since only the propbag that "belongs" to the TAS can use it to update the target directly.
//This package is used by the source's TAS when called by the source.
//The source TAS uses this package to requst the global store to make the update.
//On first call the global store calls the target to verify and then
//if ok:
//create a Visa from the Visa Request consisting of:

//1. a token that is cancellable by the target
//that is given to the source.

//2. gives the target an action it can call to cancel.
//3. gives the source and the target a shared cancelable token.)
//4. 

//This package can only be used on a request

//public:
//targetPropBag(IPropBag)
//targetPropId(PropIdType)
//sourcePropBag
//sourcePropId
//timestamp

/// </summary>

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt32, UInt32>;
    using IHaveTheKeyIT = IHaveTheKey<UInt64, UInt32, UInt32>;

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

        #region Member We Are Working On

        public object GetVisa(IPropBag requstingPropBag, PropIdType propId, IPropBag dataSource, string sourcePath)
        {
            return new object();

        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; }

        public IPropGen this[IPropBag propBag, PropIdType propId]
        {
            get
            {
                CompositeKeyType exKey = GetCompKey(propBag, propId);
                IPropGen result = _theGlobalStore[exKey];

                return result;
            }
            set
            {
                CompositeKeyType exKey = GetCompKey(propBag, propId);
                _theGlobalStore[exKey] = value;
            }
        }

        public bool TryGetValue(IPropBag propBag, PropIdType propId, out IPropGen propData)
        {
            CompositeKeyType exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryGetValue(exKey, out propData);
            return result;
        }

        public bool TryAdd(IPropBag propBag, PropIdType propId, IPropGen propData)
        {
            CompositeKeyType exKey = GetCompKey(propBag, propId);

            bool result = _theGlobalStore.TryAdd(exKey, propData);
            return result;
        }

        public bool TryRemove(IPropBag propBag, PropIdType propId, out IPropGen propData)
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

            IEnumerable<KeyValuePair<CompositeKeyType, IPropGen>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);
            foreach(KeyValuePair<CompositeKeyType, IPropGen> kvp in propDataObjectsForThisPropBag)
            {
                _theGlobalStore.TryRemove(kvp.Key, out IPropGen dontNeedItVal);
            }
        }

        public IEnumerable<KeyValuePair<PropNameType, IPropGen>> GetCollection(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<CompositeKeyType, IPropGen>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);

            IEnumerable<KeyValuePair<PropNameType, IPropGen>> result = propDataObjectsForThisPropBag.Select(x =>
                new KeyValuePair<PropNameType, IPropGen>(GetPropNameFromKey(x.Key), x.Value));

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

            IEnumerable<KeyValuePair<CompositeKeyType, IPropGen>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);

            IEnumerable<PropNameType> result = propDataObjectsForThisPropBag.Select(x =>
                GetPropNameFromKey(x.Key));

            return result;
        }

        public IEnumerable<IPropGen> GetValues(IPropBag propBag)
        {
            ObjectIdType objectId = GetAndCheckObjectRef(propBag);

            IEnumerable<KeyValuePair<CompositeKeyType, IPropGen>> propDataObjectsForThisPropBag =
                _theGlobalStore.Where(x => GetObjectIdFromKey(x.Key) == objectId);

            IEnumerable<IPropGen> result = propDataObjectsForThisPropBag.Select(x => x.Value);

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
            ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropNameType propertyName);

            return propertyName;
        }

        private ObjectIdType GetObjectIdFromKey(CompositeKeyType cKey)
        {
            ObjectIdType objectId = _compKeyManager.SplitComp(cKey, out PropIdType propId);
            return objectId;
        }

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

        #endregion

        #region Explicit Implementation of the internal interface: IHaveTheKey

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

        #endregion

    }
}
