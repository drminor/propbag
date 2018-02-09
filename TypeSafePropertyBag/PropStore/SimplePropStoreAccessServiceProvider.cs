using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    //using L2KeyManType = IL2KeyMan<UInt32, String>;

    //using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    //using Level2KeyManagerCacheInterface = ICacheLevel2KeyManagers<IL2KeyMan<UInt32, String>, UInt32, String>;
    using GenerationIdType = UInt32;


    internal class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderInterface
    {
        // TODO: This should be defined somewhere else; in some place dedicated to Level2KeyManagers.
        public const GenerationIdType GEN_ZERO = 0;

        #region Private Members

        readonly Dictionary<WeakRefKey<IPropBag>, BagNode> _store;

        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;

        //Level2KeyManagerCacheInterface _level2KeyManRepository;

        readonly object _sync = new object();
        readonly object _syncForLev2KeyMan = new object();

        const int NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS = 20;
        //private Timer _timer;

        int _accessCounter = 0; // Counts Each SetIt<T> operation on all PropBags.
        int _numOfAccessServicesCreated = 0;

        PropTemplateCache _propTemplateCache;

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


            _store = new Dictionary<WeakRefKey<IPropBag>, BagNode>();

            _propTemplateCache = new PropTemplateCache();

            //_timer = new Timer(PruneStore, null, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000);
        }

        #endregion

        #region Prune

        private void PruneStore(object stateInfo)
        {
            lock (_sync)
            {
                List<KeyValuePair<WeakRefKey<IPropBag>, BagNode>> nodesThatHavePassed = _store.Where(x => x.Value.IsAlive).ToList();

                long totalRemoved = 0;
                foreach(KeyValuePair<WeakRefKey<IPropBag>, BagNode> kvp in nodesThatHavePassed)
                {
                    totalRemoved += kvp.Value.PropertyCount;
                    kvp.Value.Parent = null;
                    _store.Remove(kvp.Key);
                }
                System.Diagnostics.Debug.WriteLine($"The PropStoreAccessServiceProvider pruned {totalRemoved} nodes at {DateTime.Now.ToLongTimeString()}.");
            }
        }

        #endregion

        #region Lookup StoreNodeBag from IPropBag

        public bool TryGetPropBagNode(IPropBag propBag, out BagNode propBagNode)
        {
            WeakRefKey<IPropBag> propBag_wrKey = new WeakRefKey<IPropBag>(propBag);

            bool result = TryGetPropBagNode(propBag_wrKey, out propBagNode);
            if(!result)
            {
                System.Diagnostics.Debug.WriteLine($"The PropStore could not retreive a BagNode for the PropBag with full classname = {propBag.FullClassName}.");
            }
            return result;
        }

        internal bool TryGetPropBagNode(WeakRefKey<IPropBag> propBag_wrKey, out BagNode propBagNode)
        {
            lock (_sync)
            {
                if (_store.TryGetValue(propBag_wrKey, out propBagNode))
                {
                    return true;
                }
            }

            propBagNode = null;
            return false;
        }

        #endregion

        #region Level2Key Management

        public object FixPropItemSet(IPropBag propBag)
        {
            object propItemSet_Handle;
            if (TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                propItemSet_Handle = FixPropItemSet(propBagNode);
            }
            else
            {
                propItemSet_Handle = null;
            }

            return propItemSet_Handle;
        }

        public object FixPropItemSet(BagNode propBagNode)
        {
            object result;

            //L2KeyManType level2KeyManager = propBagNode.Level2KeyMan;
            //if (level2KeyManager.IsFixed)
            //{
            //    System.Diagnostics.Debug.WriteLine("Warning: PropStoreAccessServiceProvider is being asked to fix an already fixed Level2Key Manager.");
            //    result = level2KeyManager;
            //}
            //else
            //{
            //    lock (_syncForLev2KeyMan)
            //    {
            //        if (_level2KeyManRepository == null) _level2KeyManRepository = new SimpleLevel2KeyManagerCache<L2KeyManType, PropIdType, PropNameType>();

            //        level2KeyManager.Fix();

            //        if (!_level2KeyManRepository.TryRegisterBaseL2KeyMan(level2KeyManager))
            //        {
            //            throw new InvalidOperationException("Could not register this PropBagNode's PropItemSet as a base PropItemSet.");
            //        }

            //        result = level2KeyManager; // We are using an instance of an internal class as an access token.
            //    }
            //}
            result = null;
            return result;
        }

        public bool TryOpenPropItemSet(IPropBag propBag, out object propItemSet_Handle)
        {
            bool result;
            if(TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                result = TryOpenPropItemSet(propBagNode, out propItemSet_Handle);
            }
            else
            {
                propItemSet_Handle = null;
                result = false;
            }

            return result;
        }

        public bool TryOpenPropItemSet(BagNode propBagNode, out object propItemSet_Handle)
        {
            bool result;

            //L2KeyManType level2KeyManager = propBagNode.Level2KeyMan;
            //if (!level2KeyManager.IsFixed)
            //{
            //    System.Diagnostics.Debug.WriteLine("Warning: PropStoreAccessServiceProvider is being asked to open a Level2Key Manager that is already open.");
            //    propItemSet_Handle = level2KeyManager;
            //    result = true;
            //}
            //else
            //{
            //    if (propBagNode.PropBagProxy.TryGetTarget(out IPropBag propBag))
            //    {
            //        if (_level2KeyManRepository == null)
            //        {
            //            throw new InvalidOperationException($"The Level2Key Manager Repository has not been initialized during call to OpenPropItemSet for PropBag:  .");
            //        }

            //        // TODO: Move these locks to the Level2Key Manager Repository
            //        // These resources are independent of the _store.
            //        lock (_syncForLev2KeyMan)
            //        {
            //            // Determine the following:
            //            // 1. If the existing Level2KeyManager has been registered.
            //            // 2. Its base, if it has one.

            //            if (_level2KeyManRepository.TryGetValueAndGenerationId(level2KeyManager, out L2KeyManType basePropItemSet, out GenerationIdType generationId))
            //            {
            //                CheckGenerationRef(level2KeyManager, basePropItemSet, generationId);

            //                L2KeyManType copy = (L2KeyManType)level2KeyManager.Clone(); // Note: the copy is open, i.e., not fixed.

            //                // Attempt to register the new, open PropItemSet as a derivative of the orignal (or its base).
            //                if (_level2KeyManRepository.TryRegisterL2KeyMan(copy, basePropItemSet, out generationId))
            //                {
            //                    // We are using an instance of a internal class as an access token.
            //                    propItemSet_Handle = copy;

            //                    // Replace the original, fixed PropSet with a new copy that is open (for the addition of new PropItems.)
            //                    propBagNode.Level2KeyMan = copy;
            //                    result = true;
            //                }
            //                else
            //                {
            //                    // TODO: Consider automatically registering the 'original' if not already registered.
            //                    throw new InvalidOperationException("Could not register the new Level2Key Manager.");
            //                }
            //            }
            //            else
            //            {
            //                throw new InvalidOperationException("The existing Level2Key Manager cannot be found in the store.");
            //            }
            //        } // end Lock(_syncForLev2KeyMan)

            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("The PropBag has been garbage collected. (During call to OpenPropItemSet.)");
            //        propItemSet_Handle = null;
            //        result = false;
            //    }

            //}

            result = false;
            propItemSet_Handle = null;
            return result;
        }

        //[System.Diagnostics.Conditional("DEBUG")]
        //private void CheckGenerationRef(L2KeyManType key, L2KeyManType basePropItemSet, GenerationIdType generationId)
        //{
        //    if(generationId == 0)
        //    {
        //        System.Diagnostics.Debug.Assert(ReferenceEquals(key, basePropItemSet), "The GenerationId is 0, but the base is not the same as the key.");
        //    }
        //    else
        //    {
        //        System.Diagnostics.Debug.Assert(!ReferenceEquals(key, basePropItemSet), "The GenerationId is not 0, but the base *is* the same as the key.");
        //    }
        //}

        #endregion

        #region PropTemplate Support

        public IPropTemplate GetOrAdd(IPropTemplate propTemplate)
        {
            IPropTemplate result = _propTemplateCache.GetOrAdd(propTemplate);
            return result;
        }

        #endregion

        #region PropStoreAccessService Creation and TearDown

        public PSAccessServiceInterface CreatePropStoreService(IPropBag propBag)
        {
            PSAccessServiceInterface result = CreatePropStoreService(propBag, null, out BagNode notUsed);
            return result;
        }

        public PSAccessServiceInterface CreatePropStoreService(IPropBag propBag, BagNode template, out BagNode newBagNode)
        {
            // Issue a new, unique Id for this propBag.
            ObjectIdType objectId = NextCookedVal;

            // Create a new PropStoreNode for this PropBag
            newBagNode = new BagNode(objectId, propBag, template, MaxPropsPerObject, _handlerDispatchDelegateCacheProvider.CallPSParentNodeChangedEventSubsCache);

            WeakRefKey<IPropBag> propBagProxy = newBagNode.PropBagProxy;

            lock (_sync)
            {
                // Add the node to the global store.
                _store.Add(propBagProxy, newBagNode);
            }

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

        public bool TearDown(BagNode propBagNode)
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
        }

        private bool TryRemoveBagNode(WeakRefKey<IPropBag> propBag_wrKey)
        {
            try
            {
                lock (_sync)
                {
                    _store.Remove(propBag_wrKey);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public PSAccessServiceInterface ClonePSAccessService
            (
            IPropBag sourcePropBag,
            BagNode sourcePropBagNode,
            IPropBag targetPropBag,
            out BagNode newStoreNode
            )
        {
            //L2KeyManType sourceLevel2KeyMan = propBagNode.Level2KeyMan; 

            //L2KeyManType level2KeyManager_forNewCopy;
            //if (sourceLevel2KeyMan.IsFixed)
            //{
            //    // Share the same instance: Fixed PropItemSets are immutable.
            //    level2KeyManager_forNewCopy = sourceLevel2KeyMan;
            //}
            //else
            //{
            //    //Make a new copy, changes to the source PropBag's Level2KeyManager will not change this new copy.
            //    level2KeyManager_forNewCopy = (L2KeyManType)sourceLevel2KeyMan.Clone();
            //}

            PSAccessServiceInterface result = CreatePropStoreService(targetPropBag, sourcePropBagNode, out newStoreNode);
            return result;
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
