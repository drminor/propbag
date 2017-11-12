using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;
    using PropNameType = String;

    public class SimplePropStoreAccessServiceProvider 
        : IProvidePropStoreAccessService<PropIdType, PropNameType>
    {
        #region Private Members

        readonly SimpleObjectIdDictionary _theGlobalStore;
        SimpleCompKeyMan _compKeyManager;
        SimpleLevel2KeyMan _level2KeyManager;

        readonly Dictionary<WeakReference<IPropBag>, ObjectIdType> _rawDict;
        readonly Dictionary<ObjectIdType, WeakReference<IPropBag>> _cookedDict;

        readonly object _sync;

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(SimpleObjectIdDictionary theGlobalStore,
            SimpleCompKeyMan compKeyManager, SimpleLevel2KeyMan level2KeyManager)
        {
            _theGlobalStore = theGlobalStore;
            _level2KeyManager = level2KeyManager;
            _compKeyManager = compKeyManager;

            MaxPropsPerObject = _compKeyManager.MaxPropsPerObject;
            MaxObjectsPerAppDomain = _compKeyManager.MaxObjectsPerAppDomain;

            _rawDict = new Dictionary<WeakReference<IPropBag>, ObjectIdType>();
            _cookedDict = new Dictionary<ObjectIdType, WeakReference<IPropBag>>();

            _sync = new object();
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; }


        public IPropStoreAccessService<PropIdType, PropNameType> GetOrCreatePropStoreService(IPropBag propBag)
        {
            ObjectIdType newObjectId = GetOrAdd(propBag, out WeakReference<IPropBag> accessToken);

            SimplePropStoreAccessService result
                = new SimplePropStoreAccessService(accessToken, newObjectId, _theGlobalStore, _compKeyManager, _level2KeyManager);

            return result;
        }

        public IPropStoreAccessService<PropIdType, PropNameType> CreatePropStoreService(IPropBag propBag)
        {
            WeakReference<IPropBag> accessToken = new WeakReference<IPropBag>(propBag);

            ObjectIdType newObjectId = Add(accessToken);

            SimplePropStoreAccessService result
                = new SimplePropStoreAccessService(accessToken, newObjectId, _theGlobalStore, _compKeyManager, _level2KeyManager);

            return result;
        }

        #endregion

        #region Private Methods

        private ObjectIdType Add(WeakReference<IPropBag> objectRef)
        {
            lock (_sync)
            {
                ObjectIdType cookedVal = NextCookedVal;
                _rawDict.Add(objectRef, cookedVal);
                _cookedDict.Add(cookedVal, objectRef);
                return cookedVal;
            }
        }

        /// <summary>
        /// This is very expensive in terms of cpu usage. Please use Add if possible.
        /// </summary>
        /// <param name="propBag"></param>
        /// <returns></returns>
        private ObjectIdType GetOrAdd(IPropBag propBag, out WeakReference<IPropBag> accessToken)
        {
            lock (_sync)
            {
                accessToken = _rawDict.Where(x => Object.ReferenceEquals(SimpleExKey.UnwrapWeakRef(x.Key), propBag)).FirstOrDefault().Key;

                if(accessToken != null)
                {
                    if(_rawDict.TryGetValue(accessToken, out ObjectIdType existingCookedValue))
                    {
                        return existingCookedValue;
                    }
                }

                // This object has not been registered, register it now.
                accessToken = new WeakReference<IPropBag>(propBag);
                ObjectIdType cookedVal = NextCookedVal;
                _rawDict.Add(accessToken, cookedVal);
                _cookedDict.Add(cookedVal, accessToken);
                return cookedVal;
            }
        }

        private long m_Counter = 0;
        private uint NextCookedVal
        {
            get
            {
                long temp = System.Threading.Interlocked.Increment(ref m_Counter);
                if (temp > MaxObjectsPerAppDomain) throw new InvalidOperationException("The SimplePropStore has run out object ids.");
                return (uint)temp;
            }
        }

        #endregion
    }
}
