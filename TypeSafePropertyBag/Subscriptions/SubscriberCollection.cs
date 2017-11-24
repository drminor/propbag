using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    public class SubscriberCollection : IEnumerable<ISubscriptionGen>
    {
        private readonly List<ISubscriptionGen> _subs;
        private readonly object _sync;

        public SubscriberCollection()
        {
            _sync = new object();
            _subs = new List<ISubscriptionGen>();
        }

        /// <summary>
        /// Removes subscriptions for which the reference object no longer lives.
        /// </summary>
        /// <param name="totalCount">The total number of subscriptions present when the operation began.</param>
        /// <returns>The number of dead subscriptions found.</returns>
        public int PruneSubscriptions(out int totalCount)
        {
            int result = 0;
            totalCount = 0;
            lock(_sync)
            {
                foreach(ISubscriptionGen x in _subs)
                {
                    //x.
                    totalCount++;
                }
            }
            return result;
        }

        public ISubscriptionGen GetOrAdd(ISubscriptionKeyGen subscriptionKey, Func<ISubscriptionKeyGen, ISubscriptionGen> factory)
        {
            lock (_sync)
            {
                if (TryGetSubscription(subscriptionKey, out ISubscriptionGen subscription))
                {
                    System.Diagnostics.Debug.WriteLine($"The subscription for {subscriptionKey.SourcePropRef} has aleady been created.");
                    return subscription;
                }
                else
                {
                    subscription = subscriptionKey.CreateSubscription();
                    AddSubscription(subscription);

                    return subscription;
                }
            }
        }

        public bool AddSubscription(ISubscriptionGen subscription)
        {
            lock (_sync)
            {
                if (ContainsSubscription(subscription))
                {
                    return false;
                }
                else
                {
                    _subs.Add(subscription);
                    return true;
                }
            }
        }

        //public bool RemoveSubscription(ISubscriptionGen subscription)
        //{
        //    lock (_sync)
        //    {
        //        if (ContainsSubscription(subscription))
        //        {
        //            _subs.Remove(subscription);
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        public bool RemoveSubscription(ISubscriptionKeyGen request)
        {
            lock (_sync)
            {
                if (TryGetSubscription(request, out ISubscriptionGen subscription))
                {
                    // TODO: consider adding a TryRemove method.
                    _subs.Remove(subscription);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the subscription for {request.SourcePropRef} when trying to remove it.");
                    return false;
                }

            }
        }

        public bool ContainsSubscription(ISubscriptionGen subscription)
        {
            bool result = _subs.Exists(x => x.Equals(subscription));
            return result;
        }

        public bool TryGetSubscription(ISubscriptionKeyGen request, out ISubscriptionGen subscription)
        {
            lock (_sync)
            {
                subscription = _subs.FirstOrDefault(x => x.Equals(request));
            }

            bool result = subscription != null;
            return result;
        }

        public int ClearSubscriptions()
        {
            int result = _subs.Count;
            _subs.Clear();

            return result;
        }

        public IEnumerator<ISubscriptionGen> GetEnumerator()
        {
            lock (_sync)
                return _subs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
