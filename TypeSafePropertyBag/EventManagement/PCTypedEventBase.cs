using System;
using System.Threading;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class PCTypedEventBase<T> : INotifyPCTyped<T>
    {
        //object _evLock;
        //private event EventHandler<PCTypedEventArgs<T>> _privateEvent;

        SimpleSubscriptionManager<IPropGen> _subscriptionManager;

        public PCTypedEventBase(SimpleSubscriptionManager<IPropGen> subscriptionManager)
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
