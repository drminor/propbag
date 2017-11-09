
using DRM.TypeSafePropertyBag.Fundamentals.ObjectIdDictionary;

namespace DRM.TypeSafePropertyBag.EventManagement
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
    /// <typeparam name="CompT">The type of the composite key for property objects.</typeparam>
    /// <typeparam name="PropDataT">The type used to cary instances of IPropGen.</typeparam>
    public interface ICacheSubscriptions<CompT, PropDataT>
    {
        ISubscriptionGen AddSubscription(ISubscriptionKeyGen subscriptionRequest, out bool wasAdded);
        bool RemoveSubscription(ISubscriptionKeyGen subscriptionRequest);

        SubscriberCollection GetSubscriptions(IExplodedKey<ulong, uint, uint> exKey);

    }
}
