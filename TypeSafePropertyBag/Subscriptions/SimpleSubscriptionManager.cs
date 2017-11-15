using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;
    using PropNameType = String;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    using ExKeyType = IExplodedKey<UInt64, UInt32, UInt32>;
    using HaveTheKeyType = IHaveTheKey<UInt64, UInt32, UInt32>;


    public class SimpleSubscriptionManager : ICacheSubscriptions<SimpleExKey, CompositeKeyType, ObjectIdType, PropIdType, PropNameType>
    {
        #region Private Members

        const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 10000;

        private ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections> _propIndexesByObject;
        //private SimpleCompKeyMan _compKeyManager;

        #endregion

        #region Constructor

        public SimpleSubscriptionManager()
        {
            //_compKeyManager = compKeyManager;
            _propIndexesByObject = new ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections>
                (concurrencyLevel: OBJECT_INDEX_CONCURRENCY_LEVEL, capacity: EXPECTED_NO_OF_OBJECTS);
        }

        #endregion

        #region Public Methods

        public ISubscriptionGen AddSubscription(ISubscriptionKeyGen subscriptionRequest, out bool wasAdded)
        {
            if(subscriptionRequest.HasBeenUsed)
            {
                throw new ApplicationException("Its alread been used.");
            }

            SubscriberCollection sc = GetSubscriptions((SimpleExKey) subscriptionRequest.SourcePropRef);

            bool internalWasAdded = false;

            ISubscriptionGen result = sc.GetOrAdd
                (
                subscriptionRequest,
                    (
                    x =>  subscriptionRequest.CreateSubscription()
                    )
                );

            if(subscriptionRequest.HasBeenUsed)
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

        public SubscriberCollection GetSubscriptions(SimpleExKey exKey)
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

        public SubscriberCollection GetSubscriptions(IPropBag host, PropIdType propId, SimplePropStoreAccessService storeAccessor)
        {
            SimpleExKey exKey = GetTheKey(host, propId, storeAccessor);

            SubscriberCollection result = GetSubscriptions(exKey);
            return result;
        }

        public SubscriberCollection GetSubscriptions(IPropBag host, PropIdType propId, PSAccessServiceType storeAccessor)
        {
            SimpleExKey exKey = GetTheKey(host, propId, storeAccessor);

            SubscriberCollection result = GetSubscriptions(exKey);
            return result;
        }

        private SimpleExKey GetTheKey(IPropBag host, uint propId, SimplePropStoreAccessService storeAccessor)
        {
            SimpleExKey result = ((IHaveTheSimpleKey)storeAccessor).GetTheKey(host, propId);

            return result;
        }

        private SimpleExKey GetTheKey(IPropBag host, uint propId, PSAccessServiceType storeAccessor)
        {
            ExKeyType exKey = ((HaveTheKeyType) storeAccessor).GetTheKey(host, propId);

            SimpleExKey withWeakRef = SimpleExKey.FromIExploadedKeyWithWeakRef(exKey);
            return withWeakRef;
        }

        #endregion

        #region Private Methods

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
    }
}
