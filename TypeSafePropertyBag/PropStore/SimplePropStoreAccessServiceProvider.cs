using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    using PSAccessServiceInternalType = IPropStoreAccessServiceInternal<UInt32, String>;
    using PSCloneServiceType = IProvidePropStoreCloneService<UInt32, String>;

    public class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderType, PSCloneServiceType
    {
        #region Private Members

        readonly Dictionary<ExKeyT, StoreNodeBag> _store;
        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;

        readonly object _sync;

        const int NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS = 20;
        //private Timer _timer;

        int _accessCounter = 0; // Counts Each SetIt<T> operation on all PropBags.
        int _numOfAccessServicesCreated = 0;

        #endregion

        #region Public Properties

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; set; }

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(int maxPropsPerObject, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            MaxPropsPerObject = maxPropsPerObject; 
            MaxObjectsPerAppDomain = GetMaxObjectsPerAppDomain(maxPropsPerObject);

            _handlerDispatchDelegateCacheProvider = handlerDispatchDelegateCacheProvider;

            _store = new Dictionary<ExKeyT, StoreNodeBag>();
            _sync = new object();
            //_timer = new Timer(PruneStore, null, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000);
        }

        #endregion

        #region Prune

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

        #endregion

        #region PropStoreAccessService Creation and TearDown

        public PSAccessServiceType CreatePropStoreService(IPropBagInternal propBag)
        {
            L2KeyManType level2KeyManager = new SimpleLevel2KeyMan(MaxPropsPerObject);
            PSAccessServiceType result = CreatePropStoreService(propBag, level2KeyManager, out StoreNodeBag notUsed);
            return result;
        }

        private PSAccessServiceType CreatePropStoreService(IPropBagInternal propBag, L2KeyManType level2KeyManager, out StoreNodeBag newBagNode)
        {
            // Issue a new, unique Id for this propBag.
            ObjectIdType objectId = NextCookedVal;

            // Create a new PropStoreNode for this PropBag
            ExKeyT cKey = new SimpleExKey(objectId, 0);
            IPropBagProxy propBagProxy = new PropBagProxy(propBag);
            newBagNode = new StoreNodeBag(cKey, propBagProxy);

            // Add the node to the global store.
            _store.Add(cKey, newBagNode);

            // Create the access service.
            PSAccessServiceType result = new SimplePropStoreAccessService
                (
                    newBagNode,
                    level2KeyManager,
                    this,
                    _handlerDispatchDelegateCacheProvider
                );

            // Add one more to the total count of PropStoreAccessServices created.
            IncAccessServicesCreated();

            return result;
        }

        public bool TearDown(ExKeyT compKey)
        {
            if(TryRemoveBagNode(compKey, out StoreNodeBag storeNodeBag))
            {
                storeNodeBag.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryRemoveBagNode(ExKeyT compKey, out StoreNodeBag storeNodeBag)
        {
            try
            {
                storeNodeBag = _store[compKey];
                _store.Remove(compKey);
                return true;
            }
            catch
            {
                storeNodeBag = null;
                return false;
            }
        }

        PSAccessServiceType PSCloneServiceType.CloneService
            (
            IPropBagInternal sourcePropBag,
            PSAccessServiceType sourceAccessService,
            IPropBagInternal targetPropBag,
            out StoreNodeBag sourceStoreNode, 
            out StoreNodeBag newStoreNode)
        {
            if(sourceAccessService is IHaveTheStoreNode nodeProvider)
            {
                sourceStoreNode = nodeProvider.PropStoreNode;

                if (((PSAccessServiceInternalType)sourcePropBag.ItsStoreAccessor).Level2KeyManager is SimpleLevel2KeyMan sourceLevel2KeyMan)
                {
                    L2KeyManType level2KeyManager_newCopy = new SimpleLevel2KeyMan(sourceLevel2KeyMan);

                    PSAccessServiceType result = CreatePropStoreService(targetPropBag, level2KeyManager_newCopy, out newStoreNode);

                    return result;
                }
                else
                {
                    throw new InvalidOperationException($"The storeNode holds a PropBag whose Level2KeyManager is not of type: {nameof(SimpleLevel2KeyMan)}.");
                }
            }
            else
            {
                throw new InvalidOperationException($"The {nameof(sourceAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
            }
        }

        #endregion

        #region Private Methods

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

        public void ResetAccessCounter()
        {
            _accessCounter = 0;
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
