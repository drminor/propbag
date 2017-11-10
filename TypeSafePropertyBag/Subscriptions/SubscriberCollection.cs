using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag.EventManagement
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

        public ISubscriptionGen GetOrAdd(ISubscriptionKeyGen subscriptionKey, Func<ISubscriptionKeyGen, ISubscriptionGen> factory)
        {
            lock (_sync)
            {
                if (TryGetSubscription(subscriptionKey.ExKey.Level2Key, out ISubscriptionGen subscription))
                {
                    System.Diagnostics.Debug.WriteLine($"The subscription for {subscriptionKey.ExKey} has aleady been created.");
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

        public bool RemoveSubscription(ISubscriptionGen subscription)
        {
            lock (_sync)
            {
                if (ContainsSubscription(subscription))
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

        public bool RemoveSubscription(IExplodedKey<ulong, uint, uint> exKey)
        {
            lock (_sync)
            {
                if (TryGetSubscription(exKey.Level2Key, out ISubscriptionGen subscription))
                {
                    // TODO: consider adding a TryRemove method.
                    _subs.Remove(subscription);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Could not find the subscription for {exKey} when trying to remove it.");
                    return false;
                }

            }
        }

        public bool ContainsSubscription(ISubscriptionGen subscription)
        {
            bool result = _subs.Contains(subscription);
            return result;
        }

        public bool TryGetSubscription(uint l2Key, out ISubscriptionGen subscription)
        {
            lock (_sync)
            {
                subscription = _subs.FirstOrDefault((x => x.ExKey.Level2Key == l2Key));
            }

            if(subscription == null)
            {
                return false;
            }
            else
            {
                return true;
            }
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
