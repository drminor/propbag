using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class CollectionOfSubscriberCollections //: IEnumerable<SubscriberCollection>
    {
        const int PROP_INDEX_CONCURRENCY_LEVEL = 1; // Typical number of threads simultaneously accessing the ObjectIndexes.
        const int EXPECTED_NO_OF_OBJECTS = 50;

        private ConcurrentDictionary<uint, SubscriberCollection> _listOfSubscribersForAProp;
        //private readonly object _sync;

        public CollectionOfSubscriberCollections()
        {
            //_sync = new object();
            _listOfSubscribersForAProp = new ConcurrentDictionary<uint, SubscriberCollection>
                (
                concurrencyLevel: PROP_INDEX_CONCURRENCY_LEVEL,
                capacity: EXPECTED_NO_OF_OBJECTS
                );
        }

        public SubscriberCollection GetOrCreate(uint l2Key, out bool wasAdded)
        {
            bool internalWasAdded = false;

            SubscriberCollection result = _listOfSubscribersForAProp.GetOrAdd
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

        public bool RemoveListOfSubscriptionPtrs(uint l2Key)
        {
            if(_listOfSubscribersForAProp.TryRemove(l2Key, out SubscriberCollection sc))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsTheListOfSubscriptionPtrs(uint l2Key)
        {
            bool result =_listOfSubscribersForAProp.ContainsKey(l2Key);
            return result;
        }

        public int ClearTheListOfSubscriptionPtrs()
        {
            int result = _listOfSubscribersForAProp.Count;
            _listOfSubscribersForAProp.Clear();

            return result;
        }

        //public IEnumerator<SubscriberCollection> GetEnumerator()
        //{
        //    lock (_sync)
        //        return _subPtrs2.ToList().GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}
    }
}

