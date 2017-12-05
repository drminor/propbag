﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DRM.TypeSafePropertyBag
{
    public class SubscriberCollection : IEnumerable<ISubscriptionGen>
    {
        private readonly object _sync;
        private readonly List<ISubscriptionGen> _subs;
        private int _serial;

        public SubscriberCollection()
        {
            _sync = new object();
            _subs = new List<ISubscriptionGen>();
            _serial = 0;
        }

        public SubscriberCollection(IEnumerable<ISubscriptionGen> subs)
        {
            _sync = new object();
            _subs = new List<ISubscriptionGen>(subs);
            _serial = 0;
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

        public ISubscriptionGen GetOrAdd(ISubscriptionKeyGen request, Func<ISubscriptionKeyGen, ISubscriptionGen> factory)
        {
            ISubscriptionGen subscription;
            lock (_sync)
            {
                if (null != (subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x, request))))
                {
                    System.Diagnostics.Debug.WriteLine($"The subscription for {request.SourcePropRef} has aleady been created.");
                    return subscription;
                }
                else
                {
                    subscription = AddSubscription(request.CreateSubscription());
                    _serial++;
                    return subscription;
                }
            }
        }

        public bool TryAddSubscription(ISubscriptionGen subscription)
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
        private ISubscriptionGen AddSubscription(ISubscriptionGen s)
        {
            SubscriptionPriorityGroup spg = s.SubscriptionPriorityGroup;

            if(_subs.Count == 0 || spg == SubscriptionPriorityGroup.Logging)
            {
                _subs.Add(s);
                return s;
            }

            int ptr = _subs.Count - 1;
            while(ptr > -1 && _subs[ptr].SubscriptionPriorityGroup > spg)
            {
                ptr--;
            }

            if (ptr == -1)
            {
                // No existing items in the target group and all existing items are part of a group that comes 'later' than the target group.
                // insert before the first item.
                _subs.Insert(0, s);
            }
            else if (ptr == _subs.Count - 1)
            {
                // Very last sub in existing list is part of the target group, insert at end.
                _subs.Add(s);
                //_subs.Insert(_subs.Count - 1, s);
            }
            else
            {
                // Insert just after found item.
                _subs.Insert(ptr + 1, s);
            }
            return s;
        }

        public bool TryRemoveSubscription(ISubscriptionGen subscription)
        {
            lock (_sync)
            {
                if (_subs.Exists(x => x.Equals(subscription)))
                {
                    _subs.Remove(subscription);
                    _serial++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool TryRemoveSubscription(ISubscriptionKeyGen request)
        {
            ISubscriptionGen subscription;
            lock (_sync)
            {
                if(null != (subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x, request))))
                {
                    _subs.Remove(subscription);
                    _serial++;
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
            bool result;

            lock (_sync) 
                result = _subs.Exists(x => x.Equals(subscription));

            return result;
        }

        public bool TryGetSubscription(ISubscriptionKeyGen request, out ISubscriptionGen subscription)
        {
            lock (_sync)
            {
                subscription = _subs.FirstOrDefault(x => SubscriptionIsForRequest(x, request));
            }

            bool result = subscription != null;
            return result;
        }

        private bool SubscriptionIsForRequest(ISubscriptionGen subscription, ISubscriptionKeyGen subscriptionRequest)
        {
            bool t1 = subscription.SourcePropRef.Equals(subscriptionRequest.SourcePropRef);
            bool t2 = subscription.Target == subscriptionRequest.Target;
            bool t3 = subscription.Method == subscriptionRequest.Method;

            bool result =
                subscription.SourcePropRef.Equals(subscriptionRequest.SourcePropRef) &&
                subscription.Target == subscriptionRequest.Target &&
                subscription.Method == subscriptionRequest.Method;

            return result;
        }

        public int ClearSubscriptions()
        {
            int result = _subs.Count;
            lock (_sync)
            {
                _subs.Clear();
                _serial++;
            }
            return result;
        }

        public IEnumerator<ISubscriptionGen> GetEnumerator()
        {
            return new SCEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public class SCEnumerator : IEnumerator<ISubscriptionGen>
        {
            SubscriberCollection _sc;
            int ptr;
            int serial;

            public SCEnumerator(SubscriberCollection sc)
            {
                _sc = sc;
                ptr = -1;
            }

            public ISubscriptionGen Current
            {
                get
                {
                    if (ptr == -1) throw new InvalidOperationException("The enumerator for the SubscriberCollection has not been started.");

                    if(serial != _sc._serial)
                    {
                        throw new InvalidOperationException("The source of the SC Enumerator has changed.");
                    }

                    return _sc._subs[ptr];
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if(ptr == -1)
                {
                    ptr = 0;
                    serial = _sc._serial;
                }
                else
                {
                    ptr++;
                }

                if(ptr < _sc._subs.Count)
                {
                    return true;
                }
                else
                {
                    Reset();
                    return false;
                }
            }

            public void Reset()
            {
                ptr = -1;
            }

            #region IDisposable Support
            protected virtual void Dispose(bool disposing) { }
            public void Dispose() => Dispose(true);
            #endregion
        }

        public class SCEnumerator_OLD : IEnumerator<ISubscriptionGen>
        {
            SubscriberCollection _sc;
            int ptr;

            int serial;
            List<ISubscriptionGen> _processed;

            public SCEnumerator_OLD(SubscriberCollection sc)
            {
                _sc = sc;
                ptr = -1;
            }

            public ISubscriptionGen Current
            {
                get
                {
                    if (ptr == -1) throw new InvalidOperationException("The enumerator for the SubscriberCollection has not been started.");

                    lock (_sc._sync)
                    {
                        if (serial != _sc._serial)
                        {
                            ptr = GetFirstUnProcessed(-1);
                            serial = _sc._serial;
                        }

                        if (ptr < _sc._subs.Count)
                        {
                            ISubscriptionGen result = _sc._subs[ptr];
                            if (!_processed.Contains(result))
                            {
                                _processed.Add(result);
                            }
                            return _sc._subs[ptr];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (ptr == -1)
                {
                    lock (_sc._sync)
                    {
                        if (_sc._subs.Count > 0)
                        {
                            serial = _sc._serial;
                            _processed = new List<ISubscriptionGen>();
                            ptr = 0;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    ptr = GetFirstUnProcessed(ptr);
                }

                if (ptr < _sc._subs.Count)
                {
                    return true;
                }
                else
                {
                    Reset();
                    return false;
                }
            }

            public void Reset()
            {
                ptr = -1;
                _processed = null;
            }

            private int GetFirstUnProcessed(int ptr)
            {
                bool foundUnProc = false;
                while (++ptr < _sc._subs.Count)
                {
                    if (!_processed.Exists(x => x.Equals(_sc._subs[ptr])))
                    {
                        _processed.Add(_sc._subs[ptr]);
                        foundUnProc = true;
                        break;
                    }
                }
                if (foundUnProc)
                {
                    return ptr;
                }
                else
                {
                    return int.MaxValue;
                }
            }

            #region IDisposable Support
            protected virtual void Dispose(bool disposing) { }
            public void Dispose() => Dispose(true);
            #endregion
        }
    }
}
