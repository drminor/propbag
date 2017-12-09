using DRM.TypeSafePropertyBag.LocalBinding;
using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the property to which this subscription will subscribe.</typeparam>
    public interface IBindingSubscription<T> : ISubscription<T>
    {
        LocalBinder<T> LocalBinder { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the property to which this subscription will subscribe.</typeparam>
    public interface ISubscription<T> : ISubscription
    {
        EventHandler<PCTypedEventArgs<T>> TypedHandler { get; }
        Action<T, T> TypedDoWhenChanged { get; }

        CallPcTypedEventSubscriberDelegate<T> TypedHandlerProxy { get; }
    }

    // Identifies a IProp using an identifer unique to the application domain
    // that raises a event of some type (from a short list of possible event types)
    // and the action to be performed when that IProp's value changes.
    public interface ISubscription
    {
        ExKeyT OwnerPropId { get; } // Identifies the object that owns the event.
        Type PropertyType { get; }

        SubscriptionKind SubscriptionKind { get; }
        SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }
        //SubscriptionTargetKind SubscriptionTargetKind { get; }

        WeakReference Target { get; }
        string MethodName { get; }

        Delegate HandlerProxy { get; }

        //Action<object, object> GenDoWhenChanged { get; }
        //Action Action { get; }

        // Binding Subscription Members
        LocalBindingInfo BindingInfo { get; }
        object LocalBinderAsObject { get; }
    }
}
