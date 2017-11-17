using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt32, UInt32>;

    public interface IBindingSubscriptionKey<T> : ISubscriptionKey<T>
    {
        // These have been moved to the ISubscriptionKeyGen interface.
        //SimpleExKey TargetPropId { get; }
        //LocalBindingInfo BindingInfo { get; }
    }

    public interface ISubscriptionKey<T> : ISubscriptionKeyGen
    {
        EventHandler<PCTypedEventArgs<T>> TypedHandler { get; }
        Action<T,T> TypedDoWhenChanged { get; }
    }

    /// <summary>
    /// When a SubscriptionKey has been used to create a subscription,
    /// HasBeenUsed is set to true, and the GenDelegate and the Target properties are set to null
    /// so as to not have the instance prevent the target from being garbage collected.
    /// </summary>
    public interface ISubscriptionKeyGen
    {
        ExKeyT SourcePropRef { get; } // Property that raises the events to which we are subscribing.

        SubscriptionKind SubscriptionKind { get; }
        SubscriptionTargetKind SubscriptionTargetKind { get; }
        SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        // If true, the target and method will be used instead of the Delegate or Action
        bool UseTargetAndMethod { get; }

        EventHandler<PropertyChangedEventArgs> StandardHandler { get; }
        EventHandler<PCGenEventArgs> GenHandler { get; }
        Action<object, object> GenDoWhenChanged { get; }
        Action Action { get; }

        object Target { get; } 
        MethodInfo Method { get; }

        ISubscriptionGen CreateSubscription();
        bool HasBeenUsed { get; }

        void MarkAsUsed();

        // Properties for BindingSubscriptions

        ExKeyT TargetPropRef { get; }
        LocalBindingInfo BindingInfo { get; }
    }
}
