using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using ObjectIdType = UInt32;
    using ObjectRefType = WeakReference<IPropBag>;

    public class SimplePropStoreAccessServiceProvider<PropBagT, PropDataT> 
        : IProvidePropStoreAccessService<PropBagT, PropDataT> where PropBagT : class, IPropBag where PropDataT : IPropGen
    {
        #region Private Members

        readonly SimpleObjectIdDictionary<PropDataT> _theGlobalStore;
        SimpleCompKeyMan _compKeyManager;
        SimpleLevel2KeyMan _level2KeyManager;

        readonly Dictionary<ObjectRefType, ObjectIdType> _rawDict;
        readonly Dictionary<ObjectIdType, ObjectRefType> _cookedDict;

        readonly object _sync;

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(SimpleObjectIdDictionary<PropDataT> theGlobalStore,
            SimpleCompKeyMan compKeyManager, SimpleLevel2KeyMan level2KeyManager)
        {
            _theGlobalStore = theGlobalStore;
            _level2KeyManager = level2KeyManager;
            _compKeyManager = compKeyManager;

            MaxPropsPerObject = _compKeyManager.MaxPropsPerObject;
            MaxObjectsPerAppDomain = _compKeyManager.MaxObjectsPerAppDomain;

            _rawDict = new Dictionary<ObjectRefType, ObjectIdType>();
            _cookedDict = new Dictionary<ObjectIdType, ObjectRefType>();

            _sync = new object();
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; }


        public IPropStoreAccessService<PropBagT, PropDataT> GetOrCreatePropStoreService(PropBagT propBag)
        {
            ObjectIdType newObjectId = GetOrAdd(propBag, out ObjectRefType accessToken);

            SimplePropStoreAccessService<PropBagT, PropDataT> result
                = new SimplePropStoreAccessService<PropBagT, PropDataT>(accessToken, newObjectId, _theGlobalStore, _compKeyManager, _level2KeyManager);

            return result;
        }

        public IPropStoreAccessService<PropBagT, PropDataT> CreatePropStoreService(PropBagT propBag)
        {
            ObjectRefType accessToken = new WeakReference<IPropBag>(propBag);

            ObjectIdType newObjectId = Add(accessToken);

            SimplePropStoreAccessService<PropBagT, PropDataT> result
                = new SimplePropStoreAccessService<PropBagT, PropDataT>(accessToken, newObjectId, _theGlobalStore, _compKeyManager, _level2KeyManager);

            return result;
        }

        #endregion

        #region Private Methods

        private ObjectIdType Add(ObjectRefType objectRef)
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
        private ObjectIdType GetOrAdd(PropBagT propBag, out ObjectRefType accessToken)
        {
            lock (_sync)
            {
                accessToken = _rawDict.Where(x => Object.ReferenceEquals(UnwrapWeakRef(x.Key), propBag)).FirstOrDefault().Key;

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

        private PropBagT UnwrapWeakRef(ObjectRefType oRef)
        {
            if(oRef.TryGetTarget(out IPropBag target))
            {
                PropBagT result = (PropBagT) target;
                return result;
            }
            else
            {
                var x = default(PropBagT);
                return x;

                //PropBag x = new PropBag(0);
                //return x as PropBagT;
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
