using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    
    using SubCacheType = ICacheSubscriptions<UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    #endregion

    public class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderType
    {
        #region Private Members

        int _accessCounter = 0; // Counts Each SetIt<T> operation on all PropBags.

        int _numOfAccessServicesCreated = 0;

        readonly PropStoreNode _tree;

        //readonly Dictionary<IPropBagProxy, ObjectIdType> _rawDict;
        //readonly Dictionary<ObjectIdType, IPropBagProxy> _cookedDict;

        readonly object _sync;

        // Subscription Management
        const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 10000;

        private ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections> _propIndexesByObject;

        const int NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS = 20;
        //private Timer _timer;

        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(int maxPropsPerObject  /*SimpleObjectIdDictionary theGlobalStore*/)
        {
            MaxPropsPerObject = maxPropsPerObject; // theGlobalStore.MaxPropsPerObject;
            MaxObjectsPerAppDomain = GetMaxObjectsPerAppDomain(maxPropsPerObject); // theGlobalStore.MaxObjectsPerAppDomain;

            // Create an "artificial" root node to hold all "real" roots, one for each "rooted" IPropBag.
            _tree = new PropStoreNode();

            //_rawDict = new Dictionary<IPropBagProxy, ObjectIdType>();
            //_cookedDict = new Dictionary<ObjectIdType, IPropBagProxy>();

            _sync = new object();

            // Create the subscription store.
            _propIndexesByObject = new ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections>
                (concurrencyLevel: OBJECT_INDEX_CONCURRENCY_LEVEL, capacity: EXPECTED_NO_OF_OBJECTS);

            //_timer = new Timer(PruneStore, null, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000, NUMBER_OF_SECONDS_BETWEEN_PRUNE_OPS * 1000);
        }

        #endregion

        private void PruneStore(object stateInfo)
        {
            lock (_sync)
            {
                List<KeyValuePair<ExKeyT, PropStoreNode>> nodesThatHavePassed = _tree.All.Where(x => x.Value.IsObjectNode && HasPassed(x.Value)).ToList();

                long totalRemoved = 0;
                foreach(KeyValuePair<ExKeyT, PropStoreNode> kvp in nodesThatHavePassed)
                {
                    totalRemoved += PruneNode(kvp.Value);
                }
                //long? numberOfNodesRemoved = _tree.All.Where(x => x.Value.IsObjectNode).Sum(x => PruneNode(x.Value));
                System.Diagnostics.Debug.WriteLine($"The PropStoreAccessServiceProvider pruned {totalRemoved} nodes at {DateTime.Now.ToLongTimeString()}.");
            }
        }

        private bool HasPassed(PropStoreNode psn)
        {
            bool result = !psn.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal dummy);
            return result;
        }

        private int PruneNode(PropStoreNode psn)
        {
            if(!psn.PropBagProxy.PropBagRef.TryGetTarget(out IPropBagInternal dummy))
            {
                int result = psn.Children.Count();
                psn.MakeItARoot(_tree);
                psn = null;
                return result;
            } 
            else
            {
                return 0;
            }
        }

        #region PropStoreAccessService Creation and TearDown

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; set; }

        public PSAccessServiceType CreatePropStoreService(IPropBagInternal propBag)
        {
            ObjectIdType objectId = NextCookedVal;
            WeakReference<IPropBagInternal> propBagRef = new WeakReference<IPropBagInternal>(propBag);
            L2KeyManType level2KeyManager = new SimpleLevel2KeyMan(MaxPropsPerObject);
            IPropBagProxy propBagProxy = new PropBagProxy(propBagRef, level2KeyManager);

            //AddToAllObjectLookups(propBagProxy);

            ExKeyT cKey = new SimpleExKey(objectId, 0);

            // Create a new PropStoreNode for this PropBag and add make it a root.
            PropStoreNode propStoreNode = new PropStoreNode(cKey, propBagProxy, _tree);

            PSAccessServiceType result = new SimplePropStoreAccessService
                (
                    propStoreNode,
                    //compKeyManager,
                    this
                );

            // Add one more to the total count of PropStoreAccessServices created.
            IncAccessServicesCreated();

            return result;
        }

        public void TearDown(IPropBag propBag, PSAccessServiceType storeAccessor)
        {
            PropStoreNode propStoreNode = ((IHaveTheStoreNode)storeAccessor).PropStoreNode;

            // Remove all subscriptions for this propBag.
            ObjectIdType objectId = propStoreNode.CompKey.Level1Key;
            bool wasRemoved = RemovePropIndexForObject(objectId);
            if(!wasRemoved)
            {
                System.Diagnostics.Debug.WriteLine($"PropBag Object: {objectId} held no subscriptions upon teardown.");
            }

            //RemoveFromAllObjectLookups(propStoreNode.PropBagProxy);
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

        #region Subscription Management

        public ISubscriptionGen AddSubscription(ISubscriptionKeyGen subscriptionRequest, out bool wasAdded)
        {
            if (subscriptionRequest.HasBeenUsed)
            {
                throw new ApplicationException("Its already been used.");
            }

            SubscriberCollection sc = GetSubscriptions((SimpleExKey)subscriptionRequest.SourcePropRef);

            ISubscriptionGen result = sc.GetOrAdd
                (
                subscriptionRequest,
                    (
                    x => subscriptionRequest.CreateSubscription()
                    )
                );

            if (subscriptionRequest.HasBeenUsed)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new Subscription for Property:" +
                    $" {subscriptionRequest.SourcePropRef} / Event: {result.SubscriptionKind}.");
                wasAdded = true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"The subscription for Property:" +
                    $" {subscriptionRequest.SourcePropRef} / Event: {result.SubscriptionKind} was not added.");
                wasAdded = false;
            }
            return result;
        }

        public bool RemoveSubscription(ISubscriptionKeyGen subscriptionRequest)
        {
            SubscriberCollection sc = GetSubscriptions(subscriptionRequest.SourcePropRef);
            bool result = sc.RemoveSubscription(subscriptionRequest);

            if (result)
                System.Diagnostics.Debug.WriteLine($"Removed the subscription for {subscriptionRequest.SourcePropRef}.");

            return result;
        }

        public SubscriberCollection GetSubscriptions(IPropBag host, uint propId)
        {
            throw new NotImplementedException();
        }

        public SubscriberCollection GetSubscriptions(ExKeyT exKey)
        {
            CollectionOfSubscriberCollections propIndex = GetPropIndexForObject(exKey.Level1Key, out bool propIndexWasCreated);
            if (propIndexWasCreated)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new CollectionOfSubscriberCollections for {exKey}.");
            }

            SubscriberCollection result = propIndex.GetOrCreate(exKey.Level2Key, out bool subcriberListWasCreated);
            if (subcriberListWasCreated)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new SubscriberCollection for {exKey}.");
            }

            return result;
        }

        #endregion

        #region Private Methods

        private CollectionOfSubscriberCollections GetPropIndexForObject(ObjectIdType objectKey, out bool wasAdded)
        {
            bool internalWasAdded = false;

            CollectionOfSubscriberCollections result = _propIndexesByObject.GetOrAdd
                (
                key: objectKey,
                valueFactory:
                    (
                    x => { internalWasAdded = true; return new CollectionOfSubscriberCollections(); }
                    )
                );

            wasAdded = internalWasAdded;
            return result;
        }

        private bool RemovePropIndexForObject(ObjectIdType objectKey)
        {
            if(_propIndexesByObject.TryRemove(objectKey, out CollectionOfSubscriberCollections dummy))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //private void AddToAllObjectLookups(IPropBagProxy propBagProxy)
        //{
        //    lock (_sync)
        //    {
        //        _rawDict.Add(propBagProxy, propBagProxy.ObjectId);
        //        _cookedDict.Add(propBagProxy.ObjectId, propBagProxy);
        //    }
        //}

        //private void RemoveFromAllObjectLookups(IPropBagProxy propBagProxy)
        //{
        //    _rawDict.Remove(propBagProxy);
        //    _cookedDict.Remove(propBagProxy.ObjectId);
        //}

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

        public int TotalNumberOfAccessServicesCreated => _numOfAccessServicesCreated;

        public int NumberOfRootPropBagsInPlay
        {
            get
            {
                return _tree.Children.Count();
            }
        }

        #endregion
    }
}
