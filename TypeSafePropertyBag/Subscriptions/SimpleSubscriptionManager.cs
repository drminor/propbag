using DRM.TypeSafePropertyBag.Fundamentals;
using System.Collections.Concurrent;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class SimpleSubscriptionManager<PropDataT> : ICacheSubscriptions<ulong, PropDataT> where PropDataT : IPropGen
    {
        #region Private Members

        const int OBJECT_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 10000;

        private ConcurrentDictionary<uint, CollectionOfSubscriberCollections> _propIndexesByObject;
        ICKeyMan<SimpleExKey, ulong, uint, uint, string> _compKeyManager;

        #endregion

        #region Constructor

        public SimpleSubscriptionManager(ICKeyMan<SimpleExKey, ulong, uint, uint, string> compKeyManager)
        {
            _compKeyManager = compKeyManager;
            _propIndexesByObject = new ConcurrentDictionary<uint, CollectionOfSubscriberCollections>
                (concurrencyLevel: OBJECT_INDEX_CONCURRENCY_LEVEL, capacity: EXPECTED_NO_OF_OBJECTS);
        }

        #endregion

        #region Public Methods

        public ISubscriptionGen AddSubscription(ISubscriptionKeyGen subscriptionRequest, out bool wasAdded)
        {
            SubscriberCollection sc = GetSubscriptions((SimpleExKey) subscriptionRequest.ExKey);

            bool internalWasAdded = false;

            ISubscriptionGen result = sc.GetOrAdd
                (
                subscriptionRequest,
                    (
                    x => { internalWasAdded = true; return subscriptionRequest.CreateSubscription(); }
                    )
                );

            if(internalWasAdded)
            {
                System.Diagnostics.Debug.WriteLine($"Created a new Subscription for Property: {subscriptionRequest.ExKey} / Event: {result.SubscriptionKind}.");
            }

            wasAdded = internalWasAdded;
            return result;
        }


        public bool RemoveSubscription(ISubscriptionKeyGen subscriptionRequest)
        {
            SubscriberCollection sc = GetSubscriptions((SimpleExKey)subscriptionRequest.ExKey);
            bool result = sc.RemoveSubscription(subscriptionRequest.ExKey);
            return result;
        }

        public SubscriberCollection GetSubscriptions(IExplodedKey<ulong, uint, uint> exKey)
        {
            CollectionOfSubscriberCollections propIndex = GetPropIndexForObject(exKey.Level1Key, out bool propIndexWasCreated);
            if(propIndexWasCreated)
            {
                System.Diagnostics.Debug.WriteLine("Created a new CollectionOfSubscriberCollections for {exKey}.");
            }

            SubscriberCollection result = propIndex.GetOrCreate(exKey.Level2Key, out bool subcriberListWasCreated);
            if(subcriberListWasCreated)
            {
                System.Diagnostics.Debug.WriteLine("Created a new SubscriberCollection for {exKey}.");
            }

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
