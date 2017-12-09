using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// An open delegate that can stand-in for any EventHandler<T>
    /// The target of the original event handler must be supplied when raising the event.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event args parameter. This type must derive from System.EventArgs.</typeparam>
    /// <param name="target">The object on which the original handler is declared.</param>
    /// <param name="sender">The object that is raising the event.</param>
    /// <param name="e">The data that is supplied by the sender for this event.</param>
    public delegate void EventHandlerProxy<TEventArgs>(object target, object sender, TEventArgs e) where TEventArgs : EventArgs;


    //public delegate void PCGenEventHandlerProxy(object target, object sender, PCGenEventArgs e);
    //public delegate void PCObjEventHandlerProxy(object target, object sender, PCObjectEventArgs e);
    //public delegate void StandardEventHandlerProxy(object target, object sender, PropertyChangedEventArgs e);


    public delegate void CallPcTypedEventSubscriberDelegate(object target, object sender, PCTypedEventArgs<object> e, Delegate d);

    public delegate void CallPcTypedEventSubscriberDelegate<T>(object target, object sender, PCTypedEventArgs<T> e, Delegate d);


    public delegate void CallPcObjEventSubscriberDelegate(object target, object sender, PcObjectEventArgs e, Delegate d);

    public delegate void CallPcGenEventSubscriberDelegate(object target, object sender, PcGenEventArgs e, Delegate d);

    public delegate void CallPcStandardEventSubscriberDelegate(object target, object sender, PropertyChangedEventArgs e, Delegate d);

    public delegate void CallPChangingEventSubscriberDelegate(object target, object sender, PropertyChangingEventArgs e, Delegate d);

}
