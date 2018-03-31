using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    // TODO: Implement IEquatable

    public class AbstractSubscripton<T> : ISubscription<T>
    {
        #region ISubscription<T> implementation

        //public Action<T, T> TypedDoWhenChanged { get; protected set; }

        #endregion

        #region ISubscription Implementation

        public ExKeyT OwnerPropId { get; protected set; }
        public Type PropertyType => typeof(T);

        public SubscriptionKind SubscriptionKind { get; protected set; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; protected set; }
        //public SubscriptionTargetKind SubscriptionTargetKind { get; protected set; }

        public object Target { get; }

        public WeakRefKey Target_Wrk { get; protected set; }
        public string MethodName { get; protected set; }

        public Delegate HandlerProxy { get; protected set; }

        public CallPcTypedEventSubscriberDelegate PcTypedHandlerDispatcher { get; protected set; }
        public CallPcGenEventSubscriberDelegate PcGenHandlerDispatcher { get; }
        public CallPcObjEventSubscriberDelegate PcObjHandlerDispatcher { get; }
        public CallPcStandardEventSubscriberDelegate PcStandardHandlerDispatcher { get; }
        public CallPChangingEventSubscriberDelegate PChangingHandlerDispatcher { get; }

        //public Action<object, object> GenDoWhenChanged => null;
        //public Action Action => null;

        // Binding Subscription Members
        public LocalBindingInfo BindingInfo { get; protected set; }
        public object LocalBinderAsObject => null;

        #endregion
    }
}
