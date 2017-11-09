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

        private ConcurrentDictionary<uint, SubscriberCollection> _subPtrs2;
        private readonly object _sync;

        public CollectionOfSubscriberCollections()
        {
            _sync = new object();
            _subPtrs2 = new ConcurrentDictionary<uint, SubscriberCollection>
                (
                concurrencyLevel: PROP_INDEX_CONCURRENCY_LEVEL,
                capacity: EXPECTED_NO_OF_OBJECTS
                );
        }

        public SubscriberCollection GetOrCreate(uint l2Key, out bool wasAdded)
        {
            bool internalWasAdded = false;

            SubscriberCollection result = _subPtrs2.GetOrAdd
                (
                l2Key,
                    (
                    x => { internalWasAdded = true; return new SubscriberCollection(); }
                    )
                );

            wasAdded = internalWasAdded;
            return result;

            //lock (_sync)
            //{
            //    if (_subPtrs2.TryGetValue(l2Key, out SubscriberCollection sc))
            //    {
            //        return sc;
            //    }
            //    else
            //    {
            //        SubscriberCollection result = new SubscriberCollection();
            //        if(_subPtrs2.TryAdd(l2Key, result))
            //        {
            //            return result;
            //        }
            //        else
            //        {
            //            throw new InvalidOperationException($"Could not add freshly created SubscriberCollection for {l2Key}.");
            //        }
            //    }
            //}
        }

        public bool RemoveListOfSubscriptionPtrs(uint l2Key)
        {
            if(_subPtrs2.TryRemove(l2Key, out SubscriberCollection sc))
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
            bool result =_subPtrs2.ContainsKey(l2Key);
            return result;
        }

        public int ClearTheListOfSubscriptionPtrs()
        {
            int result = _subPtrs2.Count;
            _subPtrs2.Clear();

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

