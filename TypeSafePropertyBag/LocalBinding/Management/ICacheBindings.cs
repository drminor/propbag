using System;
using System.Collections.Generic;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    /// <summary>
    /// Provides storage that one or more IPropBags can share to hold callbacks registered for particular properties registered on the IPropBag.
    /// The callbacks can be one of several forms including, but not limited to:
    /// EventHandlers of type: <typeparamref name="PCTypeEventArgs"/> of type: <typeparamref name="T"/>, 
    /// EventHandlers of type: <typeparamref name="PCGenEventArgs"/>,
    /// EventHandlers of type: <typeparamref name="PropertyChanged"/>,
    /// Actions of type: &lt; <typeparamref name="object"/>, <typeparamref name="T"/>, <typeparamref name="T"/> &gt;,
    /// and Actions of type: &lt; <typeparamref name="T"/>, <typeparamref name="T"/> &gt;
    /// </summary>
    /// <typeparam name="ExKeyT">The Exploded Key Type</typeparam>
    /// <typeparam name="L2T">The type used to store PropIds.</typeparam>
    public interface ICacheBindings<L2T>
    {
        ISubscription AddBinding(ISubscriptionKeyGen bindingRequest, out bool wasAdded);
        bool TryRemoveBinding(ISubscriptionKeyGen bindingRequest, out ISubscription binding);

        //bool TryRemoveBinding(IPropBag host, L2T propId);

        IEnumerable<ISubscription> GetBindings(IPropBag host, uint propId);
    }
}
