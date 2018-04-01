using System;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    //using PropNameType = String;

    using PropItemSetKeyType = PropItemSetKey<String>;
    using PropNodeCollectionInterface = IPropNodeCollection<UInt32, String>;
    using PropNodeCollectionInternalInterface = IPropNodeCollection_Internal<UInt32, String>;
    using PropNodelCollectionSharedInterface = IPropNodeCollectionShared<UInt32, String>;
    using PSAccessServiceCreatorInterface = IPropStoreAccessServiceCreator<UInt32, String>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderInterface = IProvidePropStoreAccessService<UInt32, String>;
    using PSFastAccessServiceInterface = IPropStoreFastAccess<UInt32, String>;

    internal class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderInterface
    {
        #region Private Members

        readonly Dictionary<WeakRefKey<IPropBag>, BagNode> _store;

        readonly Dictionary<PropItemSetKeyType, PropNodelCollectionSharedInterface> _storeByType;

        readonly IProvideHandlerDispatchDelegateCaches _handlerDispatchDelegateCacheProvider;

        //PropNodeCollectionCacheInterface _propNodeCollectionCache;

        readonly object _sync = new object();
        readonly object _syncForByTypeStore = new object();

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
            _storeByType = new Dictionary<PropItemSetKeyType, PropNodelCollectionSharedInterface>();

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

        public PSFastAccessServiceInterface GetFastAccessService()
        {
            return new SimplePropStoreFastAccess(this);
        }

        public object GetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey)
        {
            if (!TryGetSharedPropCollection(propItemSetKey, out PropNodelCollectionSharedInterface sharedPropCollection))
            {
                throw new InvalidOperationException($"Could not retrieve the SharedPropCollection for {propItemSetKey.FullClassName} .");
            }

            if (!sharedPropCollection.TryGetPropNode(compKey, out PropNode propNode))
            {
                throw new KeyNotFoundException($"The {sharedPropCollection} could not retrieve a PropNode for {compKey}.");
            }

            object result = propNode.PropData_Internal.TypedProp.TypedValueAsObject;
            return result;
        }

        public bool SetValueFast(ExKeyT compKey, PropItemSetKeyType propItemSetKey, object value)
        {
            if (!TryGetSharedPropCollection(propItemSetKey, out PropNodelCollectionSharedInterface sharedPropCollection))
            {
                throw new InvalidOperationException($"Could not retrieve the SharedPropCollection for {propItemSetKey.FullClassName} .");
            }

            if (!sharedPropCollection.TryGetPropNode(compKey, out PropNode propNode))
            {
                throw new KeyNotFoundException($"The {sharedPropCollection} could not retrieve a PropNode for {compKey}.");

            }

            if (!propNode.Parent.TryGetPropBag(out IPropBag propBag))
            {
                // The target has been garbage collected.
                return false;
            }

            bool result = SetPropValue(propBag, propNode, compKey.Level2Key, value);
            return result;
        }

        private bool TryGetSharedPropCollection(PropItemSetKeyType propItemSetKey, out PropNodelCollectionSharedInterface sharedPropCollection)
        {
            bool result = _storeByType.TryGetValue(propItemSetKey, out sharedPropCollection);
            return result;
        }

        public object GetValueFast(BagNode propBagNode, PropIdType propId, PropItemSetKeyType propItemSetKey)
        {
            if (propItemSetKey != propBagNode.PropItemSetKey)
            {
                throw new InvalidOperationException("Bad PropModel.");
            }

            if (!propBagNode.TryGetChild(propId, out PropNode child))
            {
                throw new InvalidOperationException("Could not retrieve PropNode for that compKey.");
            }

            object result = child.PropData_Internal.TypedProp.TypedValueAsObject;
            return result;
        }

        public bool SetValueFast(BagNode propBagNode, PropIdType propId, PropItemSetKeyType propItemSetKey, object value)
        {
            if (propItemSetKey != propBagNode.PropItemSetKey)
            {
                throw new InvalidOperationException("Bad PropModel.");
            }

            if (!propBagNode.TryGetPropBag(out IPropBag component))
            {
                throw new InvalidOperationException("IPropBag target has been Garbage Collected.");
            }

            if (!propBagNode.TryGetChild(propId, out PropNode child))
            {
                throw new InvalidOperationException("Could not retrieve PropNode for that compKey.");
            }

            bool result = SetPropValue(component, child, propId, value);
            return result;
        }

        public object GetValueFast(WeakRefKey<IPropBag> propBag_wrKey, PropIdType propId, PropItemSetKeyType propItemSetKey)
        {
            BagNode propBagNode = GetBagAndChild(propBag_wrKey, propItemSetKey, propId, out PropNode child);

            object result = child.PropData_Internal.TypedProp.TypedValueAsObject;
            return result;
        }

        public bool SetValueFast(WeakRefKey<IPropBag> propBag_wrKey, PropIdType propId, PropItemSetKeyType propItemSetKey, object value)
        {
            if (!propBag_wrKey.TryGetTarget(out IPropBag component))
            {
                // The target has been Garbage Collected, its ok to simply return false since the client is no longer waiting for our result.
                return false;
            }

            BagNode propBagNode = GetBagAndChild(propBag_wrKey, propItemSetKey, propId, out PropNode child);

            bool result = SetPropValue(component, child, propId, value);
            return result;
        }

        private BagNode GetBagAndChild(WeakRefKey<IPropBag> propBag_wrKey, PropItemSetKeyType propItemSetKey, PropIdType propId, out PropNode child)
        {
            if (!TryGetPropBagNode(propBag_wrKey, out BagNode propBagNode))
            {
                throw new InvalidOperationException("The store has lost this node.");
            }

            if (!propBag_wrKey.TryGetTarget(out IPropBag target))
            {
                throw new InvalidOperationException("IPropBag target has been Garbage Collected.");
            }

            if (propItemSetKey != propBagNode.PropItemSetKey)
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

            if (!propBagNode.TryGetChild(propId, out child))
            {
                throw new InvalidOperationException("Could not retrieve PropNode for that compKey.");
            }

            return propBagNode;
        }

        private bool SetPropValue(IPropBag component, PropNode child, PropIdType propId, object value)
        {
            try
            {
                IProp typedProp = child.PropData_Internal.TypedProp;

                string propertyName = typedProp.PropertyName;

                DoSetDelegate dsd = typedProp.PropTemplate.DoSetDelegate;
                bool result = dsd(component, propId, propertyName, typedProp, value);
                return result;
            }
            catch
            {
                throw;
            }
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
            if (TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                bool result = TryFixPropItemSet(propBagNode, propItemSetKey);
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Could not retrieve a BagNode for the given propBag.");
            }
        }

        public bool TryFixPropItemSet(BagNode propBagNode, PropItemSetKeyType propItemSetKey)
        {
            PropNodeCollectionInternalInterface pnc_int = propBagNode.PropNodeCollection;

            if (pnc_int.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Warning: PropStoreAccessServiceProvider is being asked to fix an already fixed PropItemSet.");
                return true;
            }
            else
            {
                PropNodeCollectionFixed newFixedCollection = new PropNodeCollectionFixed(pnc_int, propItemSetKey);
                propBagNode.PropNodeCollection = newFixedCollection;

                AddFixedPropCollection(newFixedCollection);
                return true;
            }
        }

        public bool TryOpenPropItemSet(IPropBag propBag)
        {
            bool result;
            if(TryGetPropBagNode(propBag, out BagNode propBagNode))
            {
                result = TryOpenPropItemSet(propBagNode);
            }
            else
            {
                result = false;
            }

            return result;
        }

        public bool TryOpenPropItemSet(BagNode propBagNode)
        {
            bool result;

            PropNodeCollectionInternalInterface pnc_int = propBagNode.PropNodeCollection;

            if (!pnc_int.IsFixed)
            {
                System.Diagnostics.Debug.WriteLine("Warning: PropStoreAccessServiceProvider is being asked to open a PropItemSet that is already open.");
                result = true;
            }
            else
            {
                // Create a new collection. (New Collections are open by default.)
                PropNodeCollectionInternalInterface newOpenCollection = new PropNodeCollection(pnc_int);

                // Remove the old set from the StoreByType
                RemoveFixedPropCollection(propBagNode.ObjectId, pnc_int);

                // Replace the exixting collection with the new one.
                propBagNode.PropNodeCollection = newOpenCollection;

                // return a reference to the new one cast as an object.
                result = true;
            }

            return result;
        }

        private PropNodelCollectionSharedInterface AddFixedPropCollection(PropNodeCollectionInternalInterface pnc)
        {
            PropItemSetKeyType propItemSetKey = pnc.PropItemSetKey;
            lock (_syncForByTypeStore)
            {
                if (_storeByType.TryGetValue(propItemSetKey, out PropNodelCollectionSharedInterface sharedPropCollection))
                {
                    sharedPropCollection.Add(pnc);
                    return sharedPropCollection;
                }
                else
                {
                    sharedPropCollection = new PropNodeCollectionShared(pnc);
                    _storeByType.Add(propItemSetKey, sharedPropCollection);
                    return sharedPropCollection;
                }
            }
        }

        private bool RemoveFixedPropCollection(ObjectIdType objectId, PropNodeCollectionInternalInterface pnc)
        {
            if(pnc.Count == 0)
            {
                return true;
            }

            PropItemSetKeyType propItemSetKey = pnc.PropItemSetKey;
            lock (_syncForByTypeStore)
            {
                if (!TryGetSharedPropCollection(propItemSetKey, out PropNodelCollectionSharedInterface sharedPropNodeCollection))
                {
                    throw new InvalidOperationException($"Could not retrieve the SharedPropCollection for {propItemSetKey.FullClassName} while removing {pnc}.");
                }

                if(sharedPropNodeCollection.TryRemove(objectId))
                {
                    if(sharedPropNodeCollection.Count == 0)
                    {
                        _storeByType.Remove(pnc.PropItemSetKey);
                    }
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not remove {pnc} from {sharedPropNodeCollection}.");
                    return false;
                }
            }
        }

        private bool DeleteSharedPropCollection(PropNodeCollectionInternalInterface pnc)
        {
            bool result = _storeByType.Remove(pnc.PropItemSetKey);
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
            ObjectIdType objectId = NextObjectId;

            // Create a new PropStoreNode for this PropBag
            newBagNode = new BagNode(objectId, propBag, propNodes, MaxPropsPerObject, _handlerDispatchDelegateCacheProvider.CallPSParentNodeChangedEventSubsCache);

            WeakRefKey<IPropBag> propBagProxy = newBagNode.PropBagProxy;

            lock (_sync)
            {
                // Add the node to the global store.
                _store.Add(propBagProxy, newBagNode);

                // If the PropNodeCollection is fixed, update our Store By Type (for fast lookups.)
                if(newBagNode.PropNodeCollection.IsFixed)
                {
                    AddFixedPropCollection(newBagNode.PropNodeCollection);
                }
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

        // TODO: Instead of performing each TearDown request synchronously, create a queue
        // for a background thread to process. This background thread can remove PropBags in batches.
        // If done in batches, ranges of Ids can be removed from the Store and StoreByType dictionaries.
        public bool TearDown(BagNode propBagNode)
        {
            //WeakRefKey<IPropBag> propBag_wrKey = propBagNode.PropBagProxy;

            if (TryRemoveBagNode(propBagNode))
            {
                propBagNode.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryRemoveBagNode(BagNode propBagNode)
        {
            try
            {
                lock (_sync)
                {
                    _store.Remove(propBagNode.PropBagProxy);

                    if (propBagNode.PropNodeCollection.IsFixed && !propBagNode.PropItemSetKey.IsEmpty)
                    {
                        if (_storeByType.TryGetValue(propBagNode.PropItemSetKey, out PropNodelCollectionSharedInterface sharedPropNodeCollection))
                        {
                            if (sharedPropNodeCollection.TryRemove(propBagNode.CompKey.Level1Key))
                            {
                                if (sharedPropNodeCollection.Count == 0)
                                {
                                    _storeByType.Remove(propBagNode.PropItemSetKey);
                                }
                            }
                            else
                            {
                                // TODO: Make this Debug.WriteLine better.
                                System.Diagnostics.Debug.WriteLine("Could not remove the child PropNodes from the Shared Prop Node Collection.");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Could not find PropBagNode in the Store By Type.");
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        //private bool TryRemoveBagNode(WeakRefKey<IPropBag> propBag_wrKey)
        //{
        //    try
        //    {
        //        lock (_sync)
        //        {
        //            _store.Remove(propBag_wrKey);
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

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

        private long m_Counter = 0; // The first ObjectId issued will be 1. (0 is reserved to indicate and empty ExplodedKey.)
        private ulong NextObjectId
        {
            get
            {
                long temp = System.Threading.Interlocked.Increment(ref m_Counter);
                if (temp > MaxObjectsPerAppDomain) throw new InvalidOperationException($"The {nameof(SimplePropStoreAccessServiceProvider)} has run out object ids.");
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
