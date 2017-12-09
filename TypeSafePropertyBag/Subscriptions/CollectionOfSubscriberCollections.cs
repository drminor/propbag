using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using System.Collections;
    using PropIdType = UInt32;

    public class CollectionOfSubscriberCollections : IEnumerable<SubscriberCollection>
    {
        const int PROP_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 50;

        private ConcurrentDictionary<PropIdType, SubscriberCollection> _subCollections;
        private readonly object _sync;

        public CollectionOfSubscriberCollections()
        {
            _sync = new object();
            _subCollections = new ConcurrentDictionary<PropIdType, SubscriberCollection>
                (
                concurrencyLevel: PROP_INDEX_CONCURRENCY_LEVEL,
                capacity: EXPECTED_NO_OF_OBJECTS
                );
        }

        public SubscriberCollection GetOrCreate(PropIdType l2Key, out bool wasAdded)
        {
            bool internalWasAdded = false;

            SubscriberCollection result = _subCollections.GetOrAdd
                (
                key: l2Key,
                valueFactory:
                    (
                    x => { internalWasAdded = true; return new SubscriberCollection(); }
                    )
                );

            wasAdded = internalWasAdded;
            return result;
        }

        public bool RemoveListOfSubscriptionPtrs(PropIdType l2Key)
        {
            if(_subCollections.TryRemove(l2Key, out SubscriberCollection sc))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsTheListOfSubscriptionPtrs(PropIdType l2Key)
        {
            bool result =_subCollections.ContainsKey(l2Key);
            return result;
        }

        public int ClearTheListOfSubscriptionPtrs()
        {
            int result = _subCollections.Count;
            _subCollections.Clear();

            return result;
        }

        public bool TryGetSubscriberCollection(PropIdType propId, out IEnumerable<ISubscription> subs)
        {
            bool result = _subCollections.TryGetValue(propId, out SubscriberCollection sc);
            if(result)
            {
                subs = sc;
                return true;
            }
            else
            {
                subs = null;
                return false;
            }
        }

        public bool TryGetSubscriberCollection(PropIdType propId, out SubscriberCollection subs)
        {
            bool result = _subCollections.TryGetValue(propId, out subs);
            return result;
        }

        public IEnumerator<SubscriberCollection> GetEnumerator()
        {
            lock (_sync)
                return _subCollections.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}

