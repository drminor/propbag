
using System;

namespace DRM.TypeSafePropertyBag
{
    /// <summary>
    /// Provides storage that one or more IPropBags can share to hold callbacks registered for particular properties registered on the IPropBag.
    /// The callbacks can be one of several forms including, but not limited to:
    /// EventHandlers of type: <typeparamref name="PCTypeEventArgs"/> of type: <typeparamref name="T"/>, 
    /// EventHandlers of type: <typeparamref name="PCGenEventArgs"/>,
    /// EventHandlers of type: <typeparamref name="PropertyChanged"/>,
    /// Actions of type: &lt; <typeparamref name="object"/>, <typeparamref name="T"/>, <typeparamref name="T"/> &gt;,
    /// and Actions of type: &lt; <typeparamref name="T"/>, <typeparamref name="T"/> &gt;
    /// </summary>
    /// <typeparam name="L2T">The type used to store PropIds.</typeparam>
    public interface IRegisterSubscriptions<L2T> : ICacheSubscriptions<L2T>
    {
        bool RegisterHandler<T>(IPropBag propBag, L2T propId, EventHandler<PCTypedEventArgs<T>> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);
        bool UnRegisterHandler<T>(IPropBag propBag, L2T propId, EventHandler<PCTypedEventArgs<T>> eventHandler);

        bool RegisterHandler(IPropBag propBag, L2T propId, EventHandler<PcGenEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);
        bool UnRegisterHandler(IPropBag propBag, L2T propId, EventHandler<PcGenEventArgs> eventHandler);

        bool RegisterHandler(IPropBag propBag, L2T propId, EventHandler<PcObjectEventArgs> eventHandler, SubscriptionPriorityGroup priorityGroup, bool keepRef);
        bool UnRegisterHandler(IPropBag propBag, L2T propId, EventHandler<PcObjectEventArgs> eventHandler);
    }
}
