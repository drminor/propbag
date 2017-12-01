using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    #endregion

    public class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderType
    {
        #region Private Members

        int _accessCounter = 0; // Counts Each SetIt<T> operation on all PropBags.

        int _numOfAccessServicesCreated = 0;

        //readonly PropStoreNode _tree;

        readonly Dictionary<ExKeyT, StoreNodeBag> _store;

        readonly object _sync;

        const int NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS = 20;
        //private Timer _timer;

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(int maxPropsPerObject)
        {
            MaxPropsPerObject = maxPropsPerObject; 
            MaxObjectsPerAppDomain = GetMaxObjectsPerAppDomain(maxPropsPerObject);

            //// Create an "artificial" root node to hold all "real" roots, one for each "rooted" IPropBag.
            //_tree = new PropStoreNode();

            ////_rawDict = new Dictionary<IPropBagProxy, ObjectIdType>();
            ////_cookedDict = new Dictionary<ObjectIdType, IPropBagProxy>();

            _store = new Dictionary<ExKeyT, StoreNodeBag>();

            _sync = new object();

            //_timer = new Timer(PruneStore, null, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000);
        }

        #endregion

        private void PruneStore(object stateInfo)
        {
            lock (_sync)
            {
                List<KeyValuePair<ExKeyT, StoreNodeBag>> nodesThatHavePassed = _store.Where(x => HasPassed(x.Value)).ToList();

                long totalRemoved = 0;
                foreach(KeyValuePair<ExKeyT, StoreNodeBag> kvp in nodesThatHavePassed)
                {
                    totalRemoved += kvp.Value.Count;
                    kvp.Value.Parent = null;
                    _store.Remove(kvp.Key);
                }
                System.Diagnostics.Debug.WriteLine($"The PropStoreAccessServiceProvider pruned {totalRemoved} nodes at {DateTime.Now.ToLongTimeString()}.");
            }
        }

        private bool HasPassed(StoreNodeBag psn)
        {
            bool result = !psn.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal dummy);
            return result;
        }

        #region PropStoreAccessService Creation and TearDown

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; set; }

        public PSAccessServiceType CreatePropStoreService(IPropBagInternal propBag)
        {
            ObjectIdType objectId = NextCookedVal;
            WeakReference<IPropBagInternal> propBagRef = new WeakReference<IPropBagInternal>(propBag);
            L2KeyManType level2KeyManager = new SimpleLevel2KeyMan(MaxPropsPerObject);
            IPropBagProxy propBagProxy = new PropBagProxy(propBagRef/*, level2KeyManager*/);

            ExKeyT cKey = new SimpleExKey(objectId, 0);

            // Create a new PropStoreNode for this PropBag
            StoreNodeBag newBag = new StoreNodeBag(cKey, propBagProxy);
            _store.Add(cKey, newBag);

            PSAccessServiceType result = new SimplePropStoreAccessService
                (
                    newBag,
                    level2KeyManager,
                    this
                );

            // Add one more to the total count of PropStoreAccessServices created.
            IncAccessServicesCreated();

            return result;
        }

        public void TearDown(ExKeyT cKey)
        {
            _store.Remove(cKey);
        }

        private IPropBagProxy GetInt_PropBag(IPropBag propBagWithInt)
        {
            if (propBagWithInt is IPropBagProxy ipbi)
            {
                return ipbi;
            }
            else
            {
                throw new ArgumentException($"int_PropBag does not implement {nameof(IPropBagProxy)}.");
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// This is very expensive in terms of cpu usage. Please use Add if possible.
        /// </summary>
        /// <param name="propBag"></param>
        /// <returns></returns>
        //private ObjectIdType GetOrAdd(IPropBag propBag, out WeakReference<IPropBag> accessToken)
        //{
        //    lock (_sync)
        //    {
        //        accessToken = _rawDict.Where(x => Object.ReferenceEquals(SimpleExKey.UnwrapWeakRef(x.Key), propBag)).FirstOrDefault().Key;

        //        if(accessToken != null)
        //        {
        //            if(_rawDict.TryGetValue(accessToken, out ObjectIdType existingCookedValue))
        //            {
        //                return existingCookedValue;
        //            }
        //        }

        //        // This object has not been registered, register it now.
        //        accessToken = new WeakReference<IPropBag>(propBag);
        //        ObjectIdType cookedVal = NextCookedVal;
        //        _rawDict.Add(accessToken, cookedVal);
        //        _cookedDict.Add(cookedVal, accessToken);
        //        return cookedVal;
        //    }
        //}

        private long m_Counter = 0;
        private ulong NextCookedVal
        {
            get
            {
                long temp = System.Threading.Interlocked.Increment(ref m_Counter);
                if (temp > MaxObjectsPerAppDomain) throw new InvalidOperationException("The SimplePropStore has run out object ids.");
                return (ulong)temp;
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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //_theGlobalStore.Clear();
                    //_tree.Clear();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Temp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #region Diagnostics

        public void IncAccess()
        {
            _accessCounter++;
        }

        public int AccessCounter => _accessCounter;

        private void IncAccessServicesCreated()
        {
            _numOfAccessServicesCreated++;
        }

        public SubscriberCollection GetSubscriptions(IPropBag host, uint propId)
        {
            throw new NotImplementedException();
        }

        public int TotalNumberOfAccessServicesCreated => _numOfAccessServicesCreated;

        public int NumberOfRootPropBagsInPlay
        {
            get
            {
                return _store.Count;
            }
        }

        #endregion
    }
}
