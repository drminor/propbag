using DRM.TypeSafePropertyBag.DelegateCaches;
using System;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;

    public interface IBindingSubscriptionKey<T> : ISubscriptionKeyGen
    {
    }

    public interface ISubscriptionKey<T> : ISubscriptionKeyGen
    {
        //EventHandler<PcTypedEventArgs<T>> TypedHandler { get; }
        //Action<T,T> TypedDoWhenChanged { get; }
    }

    /// <summary>
    /// When a SubscriptionKey has been used to create a subscription,
    /// HasBeenUsed is set to true, and the GenDelegate and the Target properties are set to null
    /// so as to not have the instance prevent the target from being garbage collected.
    /// </summary>
    public interface ISubscriptionKeyGen
    {
        Type PropertyType { get; }
        ExKeyT OwnerPropId { get; } // Property that raises the events to which we are subscribing.

        SubscriptionKind SubscriptionKind { get; }
        //SubscriptionTargetKind SubscriptionTargetKind { get; }
        SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        //EventHandler<PcGenEventArgs> GenHandler { get; }
        //EventHandler<PcObjectEventArgs> ObjHandler { get; }
        //PropertyChangedEventHandler StandardHandler { get; }
        //PropertyChangingEventHandler ChangingHandler { get; }

        //Action<object, object> GenDoWhenChanged { get; }
        //Action Action { get; }

        object Target { get; } 
        WeakRefKey Target_Wrk { get; }

        MethodInfo Method { get; }

        ISubscription CreateSubscription(IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider);
        bool HasBeenUsed { get; }

        void MarkAsUsed();

        // Properties for BindingSubscriptions
        LocalBindingInfo BindingInfo { get; }
        ISubscription CreateBinding(PSAccessServiceInterface storeAccessor); 
    }
}
