using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DRM.TypeSafePropertyBag
{
    internal class ParentNCSubscriberCollection : IEnumerable<ParentNCSubscription>, IDisposable
    {
        #region Private Properties

        private readonly object _sync;
        private readonly List<ParentNCSubscription> _subs;

        #endregion

        #region Constructors

        public ParentNCSubscriberCollection()
        {
            _sync = new object();
            _subs = new List<ParentNCSubscription>();
        }

        public ParentNCSubscriberCollection(IEnumerable<ParentNCSubscription> subs)
        {
            _sync = new object();
            _subs = new List<ParentNCSubscription>(subs);
        }

        #endregion

        #region Public Methods

        public ParentNCSubscription GetOrAdd(ParentNCSubscriptionRequest request, ICacheDelegates<CallPSParentNodeChangedEventSubDelegate> callPSParentNodeChangedEventSubsCache)
        {
            ParentNCSubscription subscription;
            lock (_sync)
            {
                if (null != (subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x,  request))))
                {
                    System.Diagnostics.Debug.WriteLine($"The subscription for {request} has aleady been created.");
                    return subscription;
                }
                else
                {
                    subscription =  request.CreateSubscription(callPSParentNodeChangedEventSubsCache);

                    if(subscription.OwnerPropId == null)
                    {
                        // TODO: See if we make this exception message more informative.
                        throw new InvalidOperationException($"OwnerPropId is null in subscription {request.Target_Wrk.GetType().ToString()}.");
                    }
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
                    subscription.Dispose();
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
                    subscription.Dispose();
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
            // Used for debugging.
            bool t1 = subscription.OwnerPropId.Equals(subscriptionRequest.OwnerPropId);
            bool t2 = subscription.MethodName == subscriptionRequest.Method.Name;
            bool t3 = subscription.Target_Wrk == subscriptionRequest.Target_Wrk;

            bool result =
                subscription.OwnerPropId.Equals(subscriptionRequest.OwnerPropId) &&
                subscription.MethodName == subscriptionRequest.Method.Name &&
                //ReferenceEquals(subscription.Target.Target, subscriptionRequest.Target);
                subscription.Target_Wrk == subscriptionRequest.Target_Wrk;

            return result;
        }

        public int ClearSubscriptions()
        {
            int result = _subs.Count;
            lock (_sync)
            {
                foreach(ParentNCSubscription parentNodeChangedSub in _subs)
                {
                    parentNodeChangedSub.Dispose();
                }

                _subs.Clear();
            }
            return result;
        }

        #endregion

        #region IEnumerable<ParentNCSubscription> Implementation

        public IEnumerator<ParentNCSubscription> GetEnumerator()
        {
            return _subs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        // TODO: Complete the PruneSubscriptions method for class: ParentNodeChangedSubscribers.
        /// <summary>
        /// Removes subscriptions for which the reference object no longer lives.
        /// </summary>
        /// <param name="totalCount">The total number of subscriptions present when the operation began.</param>
        /// <returns>The number of dead subscriptions found.</returns>
        private int PruneSubscriptions(out int totalCount)
        {
            int result = 0;
            totalCount = 0;
            lock (_sync)
            {
                foreach (ISubscription x in _subs)
                {
                    totalCount++;
                }
            }
            return result;
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
                    //Dispose managed state (managed objects).
                    ClearSubscriptions();
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
