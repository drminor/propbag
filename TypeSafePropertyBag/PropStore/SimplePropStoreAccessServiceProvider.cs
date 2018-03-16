using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using ObjectIdType = UInt64;

    using PropModelType = IPropModel<String>;
    using PropItemSetKeyType = PropItemSetKey<String>;

    using PropNodeCollectionInterface = IPropNodeCollection<UInt32, String>;
    using PropNodeCollectionInternalInterface = IPropNodeCollection_Internal<UInt32, String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;

    //using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;

    internal class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderInterface
    {
        #region Private Members

        readonly Dictionary<WeakRefKey<IPropBag>, BagNode> _store;

        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;

        //PropNodeCollectionCacheInterface _propNodeCollectionCache;

        readonly object _sync = new object();
        readonly object _syncForPropItemSetCache = new object();

        const int NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS = 20;
        //private Timer _timer;

        int _accessCounter = 0; // Counts Each SetIt<T> operation on all PropBags.
        int _numOfAccessServicesCreated = 0;

        IProvidePropTemplates _propTemplateCache;

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

            // TODO: Have the caller provide a PropTemplateCache.
            _propTemplateCache = new SimplePropTemplateCache();

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

        #region Get / Set Value Fast

        public object GetValueFast(WeakRefKey<IPropBag> propBag_wrKey, PropItemSetKeyType propItemSetKey, ExKeyT compKey)
        {
            BagNode propBagNode = GetBagAndChild(propBag_wrKey, propItemSetKey, compKey, out PropNode child);

            object result = child.PropData_Internal.TypedProp.TypedValueAsObject;
            return result;
        }

        public bool SetValueFast(WeakRefKey<IPropBag> propBag_wrKey, PropItemSetKeyType propItemSetKey, ExKeyT compKey, object value)
        {
            BagNode propBagNode = GetBagAndChild(propBag_wrKey, propItemSetKey, compKey, out PropNode child);

            if (!propBag_wrKey.TryGetTarget(out IPropBag component))
            {
                // The target has been Garbage Collected, its ok to simply return false since the client is no longer waiting for our result.
                return false;
            }

            if (!propBagNode.PropNodeCollection.TryGetPropertyName(compKey.Level2Key, out PropNameType propertyName))
            {
                throw new InvalidOperationException("Could not retrieve the Property's name using the CompKey.");
            }

            try
            {
                IProp typedProp = child.PropData_Internal.TypedProp;

                DoSetDelegate dsd = typedProp.PropTemplate.DoSetDelegate;
                bool result = dsd(component, compKey.Level2Key, propertyName, typedProp, value);
                return result;
            }
            catch
            {
                throw;
            }
        }



        private BagNode GetBagAndChild(WeakRefKey<IPropBag> propBag_wrKey, PropItemSetKeyType propItemSetId, ExKeyT compKey, out PropNode child)
        {
            if (!TryGetPropBagNode(propBag_wrKey, out BagNode propBagNode))
            {
                throw new InvalidOperationException("The store has lost this node.");
            }

            if (!propBag_wrKey.TryGetTarget(out IPropBag target))
            {
                throw new InvalidOperationException("IPropBag target has been Garbage Collected.");
            }

            if (propItemSetId != propBagNode.PropItemSetKey)
            {
                throw new InvalidOperationException("Bad PropModel.");
            }

            //if (!propItemSetId.Value.TryGetTarget(out PropModelType testModel))
            //{
            //    throw new InvalidOperationException("The specified PropItemSetId has been Garbage Collected.");
            //}

            //if (!propBagNode.PropItemSetId.Value.TryGetTarget(out PropModelType testModel_fromBagNode)) 
            //{
            //    throw new InvalidOperationException("The PropItemSetId on our BagNode has been Garbage Collected.");
            //}

            //if(!ReferenceEquals(testModel, testModel_fromBagNode))
            //{
            //    throw new InvalidOperationException("PropItemSet do not match.");
            //}

            if (!propBagNode.TryGetChild(compKey.Level2Key, out child))
            {
                throw new InvalidOperationException("Could not retrieve PropNode for that compKey.");
            }

            return propBagNode;
        }

        #endregion

        #region PropItemSet Management

        public bool IsPropItemSetFixed(IPropBag propBag)
        {
            if (TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                bool result = IsPropItemSetFixed(propBagNode);
                return result;
            }
            else
            {
                throw new InvalidOperationException("Could not access the PropBagNode for the given PropBag.");
            }
        }

        public bool IsPropItemSetFixed(BagNode propBagNode)
        {
            return propBagNode.IsFixed;
        }

        public bool TryFixPropItemSet(IPropBag propBag, PropItemSetKeyType propItemSetKey)
        {
            //object propItemSet_Handle;
            if (TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                //propItemSet_Handle = FixPropItemSet(propBagNode);
                bool result = TryFixPropItemSet(propBagNode, propItemSetKey);
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Could not retrieve a BagNode for the given propBag.");
                //return false;
                //propItemSet_Handle = null;
            }

            //return propItemSet_Handle;
        }

        public bool TryFixPropItemSet(BagNode propBagNode, PropItemSetKeyType propItemSetKey)
        {
            //object result;

            PropNodeCollectionInternalInterface pnc_int = propBagNode.PropNodeCollection;

            if (pnc_int.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Warning: PropStoreAccessServiceProvider is being asked to fix an already fixed PropItemSet.");
                //result = propBagNode.PropNodeCollection;
                return true;
            }
            else
            {
                PropNodeCollectionFixed newFixedCollection = new PropNodeCollectionFixed(pnc_int, propItemSetKey);
                propBagNode.PropNodeCollection = newFixedCollection;
                //result = newFixedCollection;
                return true;
            }

            //return result;
        }

        public bool TryOpenPropItemSet(IPropBag propBag/*, out object propItemSet_Handle*/)
        {
            bool result;
            if(TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                result = TryOpenPropItemSet(propBagNode/*, out propItemSet_Handle*/);
            }
            else
            {
                //propItemSet_Handle = null;
                result = false;
            }

            return result;
        }

        public bool TryOpenPropItemSet(BagNode propBagNode/*, out object propItemSet_Handle*/)
        {
            bool result;

            PropNodeCollectionInternalInterface pnc_int = propBagNode.PropNodeCollection;

            if (!pnc_int.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Warning: PropStoreAccessServiceProvider is being asked to open a PropItemSet that is already open.");
                //propItemSet_Handle = pnc_int;
                result = true;
            }
            else
            {
                // Create a new collection. (New Collections are open by default.)
                PropNodeCollectionInternalInterface newOpenCollection = new PropNodeCollection(pnc_int);
                
                // Replace the exixting collection with the new one.
                propBagNode.PropNodeCollection = newOpenCollection;

                // return a reference to the new one cast as an object.
                //propItemSet_Handle = newOpenCollection;
                result = true;
            }

            return result;
        }

        //[System.Diagnostics.Conditional("DEBUG")]
        //private void CheckGenerationRef(PropNodeCollectionInternalInterface key, PropNodeCollectionInternalInterface basePropItemSet, GenerationIdType generationId)
        //{
        //    if (generationId == 0)
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

        public PSAccessServiceInterface CreatePropStoreService(IPropBag propBag, PropNodeCollectionInterface propNodes)
        {
            if(propNodes is PropNodeCollectionInternalInterface pisii)
            {
                PSAccessServiceInterface propStoreService = CreatePropStoreService(propBag, pisii, out BagNode dummy);
                return propStoreService;
            }
            else
            {
                throw new InvalidOperationException("propNodes does not implement the PropItemSetInternalInterface.");
            }
        }

        public PSAccessServiceInterface CreatePropStoreService(IPropBag propBag, PropNodeCollectionInternalInterface propNodes, out BagNode newBagNode)
        {
            // Issue a new, unique Id for this propBag.
            ObjectIdType objectId = NextCookedVal;

            // Create a new PropStoreNode for this PropBag
            newBagNode = new BagNode(objectId, propBag, propNodes, MaxPropsPerObject, _handlerDispatchDelegateCacheProvider.CallPSParentNodeChangedEventSubsCache);

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
            PropNodeCollectionInternalInterface sourcePropNodeCollection,
            IPropBag targetPropBag,
            out BagNode newStoreNode
            )
        {
            PSAccessServiceInterface result = CreatePropStoreService(targetPropBag, sourcePropNodeCollection, out newStoreNode);
            return result;
        }

        #endregion

        public PSAccessServiceCreatorInterface StoreAcessorCreator => this;

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
                    // Dispose managed state (managed objects).

                    // Stop the timer, and wait for the Prune 'service' to stop.

                    lock(_sync)
                    {
                        foreach(BagNode propBagNode in _store.Values)
                        {
                            propBagNode.Dispose();
                        }
                        _store.Clear();
                        _handlerDispatchDelegateCacheProvider.Dispose();
                    }
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
