using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    internal class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderInterface
    {
        #region Private Members

        //readonly Dictionary<ExKeyT, StoreNodeBag> _store;
        readonly Dictionary<WeakRefKey<IPropBag>, StoreNodeBag> _store;

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

            _store = new Dictionary<WeakRefKey<IPropBag>, StoreNodeBag>();
              _sync = new object();
            //_timer = new Timer(PruneStore, null, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000);
        }

        #endregion

        #region Prune

        private void PruneStore(object stateInfo)
        {
            lock (_sync)
            {
                List<KeyValuePair<WeakRefKey<IPropBag>, StoreNodeBag>> nodesThatHavePassed = _store.Where(x => x.Value.IsAlive).ToList();

                long totalRemoved = 0;
                foreach(KeyValuePair<WeakRefKey<IPropBag>, StoreNodeBag> kvp in nodesThatHavePassed)
                {
                    totalRemoved += kvp.Value.Count;
                    kvp.Value.Parent = null;
                    _store.Remove(kvp.Key);
                }
                System.Diagnostics.Debug.WriteLine($"The PropStoreAccessServiceProvider pruned {totalRemoved} nodes at {DateTime.Now.ToLongTimeString()}.");
            }
        }

        #endregion

        #region Lookup StoreNodeBag from IPropBag

        public bool TryGetPropBagNode(IPropBag propBag, out StoreNodeBag propBagNode)
        {
            WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(propBag);

            bool result = TryGetPropBagNode(propBag_wrKey, out propBagNode);
            return result;
        }

        internal bool TryGetPropBagNode(WeakRefKey<IPropBag> propBag_wrKey, out StoreNodeBag propBagNode)
        {
            if (_store.TryGetValue(propBag_wrKey, out propBagNode))
            {
                return true;
            }
            else
            {
                propBagNode = null;
                return false;
            }
        }

        #endregion

        #region PropStoreAccessService Creation and TearDown

        public PSAccessServiceInterface CreatePropStoreService(IPropBag propBag)
        {
            L2KeyManType level2KeyManager = new SimpleLevel2KeyMan(MaxPropsPerObject);
            PSAccessServiceInterface result = CreatePropStoreService(propBag, level2KeyManager, out StoreNodeBag notUsed);
            return result;
        }

        private PSAccessServiceInterface CreatePropStoreService(IPropBag propBag, L2KeyManType level2KeyManager, out StoreNodeBag newBagNode)
        {
            // Issue a new, unique Id for this propBag.
            ObjectIdType objectId = NextCookedVal;

            // Create a new PropStoreNode for this PropBag
            ExKeyT cKey = new SimpleExKey(objectId, 0);
            newBagNode = new StoreNodeBag(cKey, propBag, level2KeyManager, _handlerDispatchDelegateCacheProvider.CallPSParentNodeChangedEventSubsCache);

            // Add the node to the global store.
            _store.Add(newBagNode.PropBagProxy, newBagNode);

            // Create the access service.
            PSAccessServiceInterface result = new SimplePropStoreAccessService
                (
                    newBagNode,
                    this,
                    _handlerDispatchDelegateCacheProvider
                );

            // Add one more to the total count of PropStoreAccessServices created.
            IncAccessServicesCreated();

            return result;
        }

        public bool TearDown(StoreNodeBag propBagNode)
        {
            WeakRefKey<IPropBag> propBag_wrKey = propBagNode.PropBagProxy;

            if (TryRemoveBagNode(propBag_wrKey))
            {
                propBagNode.Dispose();
                return true;
            }
            else
            {
                return false;
            }


            //if (propBagNode.PropBagProxy .TryGetPropBag(out IPropBagInternal ipbi))
            //{
            //    WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(ipbi);
            //    if (TryRemoveBagNode(propBag_wrKey, out StoreNodeBag dummy))
            //    {
            //        propBagNode.Dispose();
            //        return true;
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine($"The propBagNode references a IPropBag that has been garbage collected.");
            //    return false;
            //}
        }

        private bool TryRemoveBagNode(WeakRefKey<IPropBag> propBag_wrKey/*, out StoreNodeBag storeNodeBag*/)
        {
            try
            {
                //storeNodeBag = _store[propBag_wrKey];
                _store.Remove(propBag_wrKey);
                return true;
            }
            catch
            {
                //storeNodeBag = null;
                return false;
            }
        }

        public PSAccessServiceInterface ClonePSAccessService
            (
            IPropBag sourcePropBag,
            PSAccessServiceInterface sourceAccessService,
            L2KeyManType sourceLevel2KeyMan,
            IPropBag targetPropBag,
            out StoreNodeBag newStoreNode
            )
        {
            L2KeyManType level2KeyManager_newCopy = (L2KeyManType) sourceLevel2KeyMan.Clone();
            PSAccessServiceInterface result = CreatePropStoreService(targetPropBag, level2KeyManager_newCopy, out newStoreNode);
            return result;


            //if (sourceAccessService is IHaveTheStoreNode nodeProvider)
            //{
            //    sourceStoreNode = nodeProvider.PropStoreNode;

            //    if (((PSAccessServiceInternalInterface)sourcePropBag.ItsStoreAccessor).Level2KeyManager is SimpleLevel2KeyMan sourceLevel2KeyMan)
            //    {
            //        L2KeyManType level2KeyManager_newCopy = new SimpleLevel2KeyMan(sourceLevel2KeyMan);

            //        PSAccessServiceInterface result = CreatePropStoreService(targetPropBag, level2KeyManager_newCopy, out newStoreNode);

            //        return result;
            //    }
            //    else
            //    {
            //        throw new InvalidOperationException($"The storeNode holds a PropBag whose Level2KeyManager is not of type: {nameof(SimpleLevel2KeyMan)}.");
            //    }
            //}
            //else
            //{
            //    throw new InvalidOperationException($"The {nameof(sourceAccessService)} does not implement the {nameof(IHaveTheStoreNode)} interface.");
            //}
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
