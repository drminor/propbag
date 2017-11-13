using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag.UnUsed
{
    public class PCTypedEventBase<TPropData, T> : INotifyPCTyped<T> where TPropData : IPropGen
    {
        //object _evLock;
        //private event EventHandler<PCTypedEventArgs<T>> _privateEvent;

        SimpleSubscriptionManager _subscriptionManager;

        public PCTypedEventBase(SimpleSubscriptionManager subscriptionManager)
        {
            //_evLock = new object();
            //_privateEvent = delegate { };
            _subscriptionManager = subscriptionManager;
        }

        public event EventHandler<PCTypedEventArgs<T>> PropertyChangedWithTVals
        {
            add
            {
                //lock (_evLock)
                //{
                //    //value.
                //    _privateEvent += value;
                //}

            }
            remove
            {
                //lock (_evLock)
                //{
                //    _privateEvent -= value;
                //}
            }
        }

        //public void OnPropertyChangedWithTVals(string propertyName, T oldVal, T newVal)
        //{
        //    EventHandler<PCTypedEventArgs<T>> ourEvent = Interlocked.CompareExchange(ref _privateEvent, null, null);

        //    EventHandler<PCTypedEventArgs<T>>[] invocList = null;
        //    if (ourEvent == null) return;


        //    lock (_evLock)
        //    {
        //        invocList = (EventHandler<PCTypedEventArgs<T>>[]) ourEvent.GetInvocationList();
        //    }

        //    PCTypedEventArgs<T> eArgs = new PCTypedEventArgs<T>(propertyName, oldVal, newVal);

        //    foreach(EventHandler<PCTypedEventArgs<T>> handlerMethod in invocList)
        //    {
        //        handlerMethod(this, eArgs);
        //    }
        //}


    }
}
