using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    using ICKeyManType = ICKeyMan<UInt64, UInt32, UInt32, String>;

    public class SimplePropStoreAccessServiceProvider 
        : IProvidePropStoreAccessService<PropIdType, PropNameType>
    {
        #region Private Members

        readonly SimpleObjectIdDictionary _theGlobalStore;
        //SimpleCompKeyMan _compKeyManager;
        //SimpleLevel2KeyMan _level2KeyManager;

        readonly Dictionary<WeakReference<IPropBag>, ObjectIdType> _rawDict;
        readonly Dictionary<ObjectIdType, WeakReference<IPropBag>> _cookedDict;

        readonly object _sync;

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(SimpleObjectIdDictionary theGlobalStore/*, int maxPropsPerObject*/)
        {
            _theGlobalStore = theGlobalStore;
            //_level2KeyManager = level2KeyManager;
            //_compKeyManager = compKeyManager;

            MaxPropsPerObject = theGlobalStore.CompKeyManager.MaxPropsPerObject; //  maxPropsPerObject; // _compKeyManager.MaxPropsPerObject;
            MaxObjectsPerAppDomain = theGlobalStore.CompKeyManager.MaxObjectsPerAppDomain;

            //GetMaxObjectsPerAppDomain(maxPropsPerObject); // _compKeyManager.MaxObjectsPerAppDomain;

            _rawDict = new Dictionary<WeakReference<IPropBag>, ObjectIdType>();
            _cookedDict = new Dictionary<ObjectIdType, WeakReference<IPropBag>>();

            _sync = new object();
        }

        #endregion

        #region Public Members

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; private set; }


        public IPropStoreAccessService<PropIdType, PropNameType> GetOrCreatePropStoreService(IPropBag propBag, L2KeyManType level2KeyManager)
        {
            ObjectIdType newObjectId = GetOrAdd(propBag, out WeakReference<IPropBag> accessToken);

            // TODO: do we have the PropStoreAccesService use a shared instance of the Composite Key Manager,
            // or do we create a new instance for each service?
            // If shared, do we need to aquire locks during the bit manipulation operations?
            // If so what performance loss do we get from having to aquire those lock?

            ICKeyManType compKeyManager = new SimpleCompKeyMan(level2KeyManager.MaxPropsPerObject /*level2KeyManager*/);
            //ICKeyManType compKeyManager = _theGlobalStore.CompKeyManager;

            SimplePropStoreAccessService result
                = new SimplePropStoreAccessService(accessToken, newObjectId, _theGlobalStore, compKeyManager, level2KeyManager);

            return result;
        }

        public IPropStoreAccessService<PropIdType, PropNameType> CreatePropStoreService(IPropBag propBag, L2KeyManType level2KeyManager)
        {
            WeakReference<IPropBag> accessToken = new WeakReference<IPropBag>(propBag);

            ObjectIdType newObjectId = Add(accessToken);

            ICKeyManType compKeyManager = new SimpleCompKeyMan(level2KeyManager.MaxPropsPerObject /*level2KeyManager*/);
            SimplePropStoreAccessService result
                = new SimplePropStoreAccessService(accessToken, newObjectId, _theGlobalStore, compKeyManager, level2KeyManager);

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

        private long GetMaxObjectsPerAppDomain(int maxPropsPerObject)
        {
            double numBitsForProps = Math.Log(maxPropsPerObject, 2);

            if ((int)numBitsForProps - numBitsForProps > 0.5)
            {
                throw new ArgumentException("The maxPropsPerObject must be an even power of two. For example: 256, 512, 1024, etc.");
            }

            int numberOfBitsInCKey = (int)Math.Log(CompositeKeyType.MaxValue, 2);

            double topRange = numberOfBitsInCKey - 2; // Must leave room for at least 4 objects.

            if (4 > numBitsForProps || numBitsForProps > topRange)
            {
                throw new ArgumentException($"maxPropsPerObject must be between 4 and {topRange}, inclusive.", nameof(maxPropsPerObject));
            }

            int numberOfTopBits = (int)Math.Round((double)numberOfBitsInCKey - numBitsForProps, 0);
            long maxObjectsPerAppDomain = (long)Math.Pow(2, numberOfTopBits);

            return maxObjectsPerAppDomain;
        }

        #endregion
    }
}
