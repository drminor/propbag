﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    public class ParentNodeChangedSubscribers : IEnumerable<ParentNCSubscription>
    {
        private readonly object _sync;
        private readonly List<ParentNCSubscription> _subs;

        public ParentNodeChangedSubscribers()
        {
            _sync = new object();
            _subs = new List<ParentNCSubscription>();
        }

        public ParentNodeChangedSubscribers(IEnumerable<ParentNCSubscription> subs)
        {
            _sync = new object();
            _subs = new List<ParentNCSubscription>(subs);
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
                foreach(ISubscription x in _subs)
                {
                    totalCount++;
                }
            }
            return result;
        }

        public ParentNCSubscription GetOrAdd(ParentNCSubscriptionRequest request, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            ParentNCSubscription subscription;
            lock (_sync)
            {
                if (null != (subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x,  request))))
                {
                    System.Diagnostics.Debug.WriteLine($"The subscription for {request.OwnerPropId} has aleady been created.");
                    return subscription;
                }
                else
                {
                    subscription = new ParentNCSubscription(request, handlerDispatchDelegateCacheProvider);
                    AddSubscription(subscription);
                    return subscription;
                }
            }
        }

        public bool TryAddSubscription(ParentNCSubscription subscription)
        {
            lock (_sync)
            {
                if (_subs.Contains(subscription))
                {
                    return false;
                }
                else
                {
                    AddSubscription(subscription);
                    return true;
                }
            }
        }

        // Keep items of the same target group together and 
        // makes sure that all items in a lower numbered target group come before items in a higher numbered group.
        // Adds new items at the 'end' of it's target group.
        private ParentNCSubscription AddSubscription(ParentNCSubscription s)
        {
            _subs.Add(s);
            return s;
        }


        public bool TryRemoveSubscription(ParentNCSubscription subscription)
        {
            lock (_sync)
            {
                if (_subs.Exists(x => x.Equals(subscription)))
                {
                    _subs.Remove(subscription);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TryRemoveSubscription(ParentNCSubscriptionRequest request)
        {
            ParentNCSubscription subscription;
            lock (_sync)
            {
                if(null != (subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x, request))))
                {
                    _subs.Remove(subscription);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the subscription for {request.OwnerPropId} when trying to remove it.");
                    return false;
                }
            }
        }

        public bool ContainsSubscription(ISubscription subscription)
        {
            bool result;

            lock (_sync) 
                result = _subs.Exists(x => x.Equals(subscription));

            return result;
        }

        public bool TryGetSubscription(ParentNCSubscriptionRequest request, out ParentNCSubscription subscription)
        {
            lock (_sync)
            {
                subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x, request));
            }

            bool result = subscription != null;
            return result;
        }

        private bool SubscriptionIsForRequest(ParentNCSubscription subscription, ParentNCSubscriptionRequest subscriptionRequest)
        {
            bool t1 = subscription.OwnerPropId.Equals(subscriptionRequest.OwnerPropId);
            bool t2 = subscription.MethodName == subscriptionRequest.Method.Name;
            bool t3 = ReferenceEquals(subscription.Target.Target, subscriptionRequest.Target);

            bool result =
                subscription.OwnerPropId.Equals(subscriptionRequest.OwnerPropId) &&
                subscription.MethodName == subscriptionRequest.Method.Name &&
                ReferenceEquals(subscription.Target.Target, subscriptionRequest.Target);

            return result;
        }

        public int ClearSubscriptions()
        {
            int result = _subs.Count;
            lock (_sync)
            {
                _subs.Clear();
            }
            return result;
        }

        public IEnumerator<ParentNCSubscription> GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing) { }
        public void Dispose() => Dispose(true);

        #endregion
    }
}