using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// An open delegate used to call EventHandler of type PCTypedEventArgs."/>
    /// The target of the original event handler must be supplied when raising the event.
    /// </summary>
    /// <typeparam name="T">The type of property value for the PropItem being listened to.
    /// <param name="target">The object on which the original handler is declared.</param>
    /// <param name="sender">The object that is raising the event.</param>
    /// <param name="e">The data that is supplied by the sender for this event.</param>
    //public delegate void CallPcTypedEventSubscriberDelegate<T>(object target, object sender, PCTypedEventArgs<T> e, Delegate d);

    public delegate void CallPcTypedEventSubscriberDelegate(object target, object sender, object e, Delegate d);

    /// <summary>
    /// An open delegate used to call an EventHandler of Type PcGenEventArgs.
    /// The target of the original event handler must be supplied when raising the event.
    /// </summary>
    /// <typeparam name="T">The type of property value for the PropItem being listened to.
    /// <param name="target">The object on which the original handler is declared.</param>
    /// <param name="sender">The object that is raising the event.</param>
    /// <param name="e">The data that is supplied by the sender for this event.</param>
    public delegate void CallPcGenEventSubscriberDelegate(object target, object sender, PcGenEventArgs e, Delegate d);

    public delegate void CallPcObjEventSubscriberDelegate(object target, object sender, PcObjectEventArgs e, Delegate d);

    public delegate void CallPcStandardEventSubscriberDelegate(object target, object sender, PropertyChangedEventArgs e, Delegate d);

    public delegate void CallPChangingEventSubscriberDelegate(object target, object sender, PropertyChangingEventArgs e, Delegate d);







}
