using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt32;
    using PropIdType = UInt32;

    public class SimpleSubscriptionManager<PropDataT> : ICacheSubscriptions<SimpleExKey, CompositeKeyType, ObjectIdType, PropIdType, PropDataT> where PropDataT : IPropGen
    {
        #region Private Members

        const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 10000;

        private ConcurrentDictionary<ObjectIdType, CollectionOfSubscriberCollections> _propIndexesByObject;
        private SimpleCompKeyMan _compKeyManager;

        #endregion

        #region Constructor

        public SimpleSubscriptionManager(SimpleCompKeyMan compKeyManager)
        {
            _compKeyManager = compKeyManager;
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

            SubscriberCollection sc = GetSubscriptions((SimpleExKey) subscriptionRequest.SourcePropId);

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
                    $" {subscriptionRequest.SourcePropId} / Event: {result.SubscriptionKind}.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"The subscription for for Property:" +
                    $" {subscriptionRequest.SourcePropId} / Event: {result.SubscriptionKind} was not added.");
            }

            wasAdded = internalWasAdded;
            return result;
        }

        public bool RemoveSubscription(ISubscriptionKeyGen subscriptionRequest)
        {
            SubscriberCollection sc = GetSubscriptions((SimpleExKey)subscriptionRequest.SourcePropId);
            bool result = sc.RemoveSubscription(subscriptionRequest);

            if (result)
                System.Diagnostics.Debug.WriteLine($"Removed the subscription for {subscriptionRequest.SourcePropId}.");

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

        public SubscriberCollection GetSubscriptions
            (
            IPropBag host,
            PropIdType propId,
            SimplePropStoreAccessService<IPropBag, IPropGen> storeAccessor
            )
        {
            SimpleExKey exKey = GetTheKey(host, propId, storeAccessor);

            SubscriberCollection result = GetSubscriptions(exKey);
            return result;
        }

        private SimpleExKey GetTheKey(IPropBag host, uint propId, SimplePropStoreAccessService<IPropBag, IPropGen> storeAccessor)
        {
            SimpleExKey result = ((IHaveTheKey<IPropBag>)storeAccessor).GetTheKey(host, propId);

            return result;
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
