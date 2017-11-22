using DRM.TypeSafePropertyBag.LocalBinding;

using System;
using System.ComponentModel;
using System.Reflection;

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
    public interface ISubscription<T> : ISubscriptionGen
    {
        // Note: only one of the Eventhandlers or one of the Actions will have a value.
        // The SubscriptionKind specifies which one will be used.
        EventHandler<PCTypedEventArgs<T>> TypedHandler { get; }
        Action<T, T> TypedDoWhenChanged { get; }
    }

    // Identifies a IProp using an identifer unique to the application domain
    // that raises a event of some type (from a short list of possible event types)
    // and the action to be performed when that IProp's value changes.
    public interface ISubscriptionGen
    {
        ExKeyT SourcePropId { get; } // A weak reference to the object that raises the event.
        Type PropertyType { get; }

        SubscriptionKind SubscriptionKind { get; }
        SubscriptionTargetKind SubscriptionTargetKind { get; }
        SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        EventHandler<PCGenEventArgs> GenHandler { get; }
        EventHandler<PropertyChangedEventArgs> StandardHandler { get; }
        Action<object, object> GenDoWhenChanged { get; }
        Action Action { get; }

        object Target { get; }
        MethodInfo Method { get; }

        // Binding Subscription Members
        ExKeyT TargetPropId { get; }
        LocalBindingInfo BindingInfo { get; }
    }

}
