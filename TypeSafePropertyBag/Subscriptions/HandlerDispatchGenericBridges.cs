using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    // TODO: Consider using a single bridge (and single delegate and cache) for each of these.
    // A new parameter: subscriptionKind would be supplied so that the correct cast and call would be made.
    public class HandlerDispatchGenericBridges
    {
        private void CallPcTypedEventSubscriber<TCaller, T>(object target, object sender, object e, Delegate d)
        {
            Action<TCaller, object, PcTypedEventArgs<T>> realDel = (Action<TCaller, object, PcTypedEventArgs<T>>)d;

            realDel((TCaller)target, sender, (PcTypedEventArgs<T>) e);
        }

        private void CallPcGenEventSubscriber<TCaller>(object target, object sender, PcGenEventArgs e, Delegate d)
        {
            Action<TCaller, object, PcGenEventArgs> realDel = (Action<TCaller, object, PcGenEventArgs>)d;

            realDel((TCaller)target, sender, e);
        }

        private void CallPcObjectEventSubscriber<TCaller>(object target, object sender, PcObjectEventArgs e, Delegate d)
        {
            Action<TCaller, object, PcObjectEventArgs> realDel = (Action<TCaller, object, PcObjectEventArgs>)d;

            realDel((TCaller)target, sender, e);
        }

        private void CallPcStEventSubscriber<TCaller>(object target, object sender, PropertyChangedEventArgs e, Delegate d)
        {
            Action<TCaller, object, PropertyChangedEventArgs> realDel = (Action<TCaller, object, PropertyChangedEventArgs>)d;

            realDel((TCaller)target, sender, e);
        }

        private void CallPChangingEventSubscriber<TCaller>(object target, object sender, PropertyChangingEventArgs e, Delegate d)
        {
            Action<TCaller, object, PropertyChangingEventArgs> realDel = (Action<TCaller, object, PropertyChangingEventArgs>)d;

            realDel((TCaller)target, sender, e);
        }
    }
}
