using System;
using System.Collections.Generic;
using System.Linq;
using DRM.TypeSafePropertyBag.Fundamentals.GenericTree;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag
{
    #region Type Aliases
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;
    using PropNameType = String;

    using L2KeyManType = IL2KeyMan<UInt32, String>;
    using ICKeyManType = ICKeyMan<UInt64, UInt32, UInt32, String>;

    using ExKeyT = IExplodedKey<UInt64, UInt32, UInt32>;
    using IHaveTheKeyIT = IHaveTheKey<UInt64, UInt32, UInt32>;

    using SubCacheType = ICacheSubscriptions<UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;
    using PSAccessServiceProviderType = IProvidePropStoreAccessService<UInt32, String>;
    #endregion

    public class SimplePropStoreAccessServiceProvider : PSAccessServiceProviderType
    {
        #region Private Members

        int _accessCounter = 0; // Counts Each SetIt<T> operation on all PropBags.

        int _numOfAccessServicesCreated = 0;

        SubCacheType _subscriptionManager;

        readonly SimpleObjectIdDictionary _theGlobalStore;

        readonly Node<NodeData> _tree;

        readonly Dictionary<WeakReference<IPropBag>, ObjectIdType> _rawDict;
        readonly Dictionary<ObjectIdType, WeakReference<IPropBag>> _cookedDict;

        readonly object _sync;

        // Subscription Management
        const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 10000;

        private ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections> _propIndexesByObject;


        #endregion

        #region Constructor

        public SimplePropStoreAccessServiceProvider(SimpleObjectIdDictionary theGlobalStore, SubCacheType subscriptionManager)
        {
            _theGlobalStore = theGlobalStore;
            _subscriptionManager = subscriptionManager;

            // Create an "artificial" root node to hold all "real" roots, one for each "rooted" IPropBag.
            // This node is a PropType node; its IsObjectNode = false. It has value 0 for its ObjectId, and 0 for its PropId.
            _tree = new Node<NodeData>(new NodeData(0, new PropGen()));

            MaxPropsPerObject = theGlobalStore.MaxPropsPerObject; 
            MaxObjectsPerAppDomain = theGlobalStore.MaxObjectsPerAppDomain;

            _rawDict = new Dictionary<WeakReference<IPropBag>, ObjectIdType>();
            _cookedDict = new Dictionary<ObjectIdType, WeakReference<IPropBag>>();

            _sync = new object();

            _propIndexesByObject = new ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections>
                (concurrencyLevel: OBJECT_INDEX_CONCURRENCY_LEVEL, capacity: EXPECTED_NO_OF_OBJECTS);
        }

        #endregion

        #region PropStoreAccessService Creation and TearDown

        public int MaxPropsPerObject { get; }
        public long MaxObjectsPerAppDomain { get; set; }

        public PSAccessServiceType GetOrCreatePropStoreService(IPropBag propBag, L2KeyManType level2KeyManager)
        {
            ObjectIdType newObjectId = GetOrAdd(propBag, out WeakReference<IPropBag> accessToken);

            PSAccessServiceType result = CreatePropStoreService(propBag, level2KeyManager, accessToken, newObjectId);
            return result;
        }

        public PSAccessServiceType CreatePropStoreService(IPropBag propBag, L2KeyManType level2KeyManager)
        {
            WeakReference<IPropBag> accessToken = new WeakReference<IPropBag>(propBag);
            ObjectIdType newObjectId = Add(accessToken);

            PSAccessServiceType result = CreatePropStoreService(propBag, level2KeyManager, accessToken, newObjectId);
            return result;
        }

        // TODO: Throw an exception if the level2KeyManager's MaxPropsPerObject doesn't match the GloblStore's value.
        private PSAccessServiceType CreatePropStoreService(IPropBag propBag, L2KeyManType level2KeyManager,
             WeakReference<IPropBag> accessToken, ObjectIdType newObjectId)
        {
            if(level2KeyManager.MaxPropsPerObject != _theGlobalStore.MaxPropsPerObject)
            {
                throw new ArgumentException($"The level2KeyManager has a value for MaxPropsPerObject ({level2KeyManager.MaxPropsPerObject})" +
                    $" that is different from the GlobalStore (MaxPropPerObject: {_theGlobalStore.MaxPropsPerObject}) " +
                    $"being used by this SimplePropStoreAccessServiceProvider.");
            }

            ICKeyManType compKeyManager = new SimpleCompKeyMan(level2KeyManager.MaxPropsPerObject);

            PSAccessServiceType result = new SimplePropStoreAccessService(accessToken, newObjectId,
                _theGlobalStore, compKeyManager, level2KeyManager, this);

            // Add a new ObjectType node to our tree for the rooted IPropBag.
            NodeData nodeData = new NodeData(newObjectId, result);
            Node<NodeData> theNewNode = _tree.Add(nodeData);

            // Allows the service to add / remove nodes from its node.
            ((SimplePropStoreAccessService)result)._ourNodeFromGlobalTree = theNewNode;

            // Add one more to the total count of PropStoreAccessServices created.
            IncAccessServicesCreated();

            return result;
        }

        public void TearDown(PSAccessServiceType propStoreAccessService)
        {
            ObjectIdType objectId = ((IHaveTheKeyIT)propStoreAccessService).ObjectId;

            if(_cookedDict.TryGetValue(objectId, out WeakReference<IPropBag> WR_AccessToken))
            {
                _rawDict.Remove(WR_AccessToken);
                _cookedDict.Remove(objectId);
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

            bool internalWasAdded = false;

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
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"The subscription for for Property:" +
                    $" {subscriptionRequest.SourcePropRef} / Event: {result.SubscriptionKind} was not added.");
            }

            wasAdded = internalWasAdded;
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

        private CollectionOfSubscriberCollections GetPropIndexForObject(uint objectKey, out bool wasAdded)
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

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _theGlobalStore.Clear();
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
    }
}
