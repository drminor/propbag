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

        public EventHandler<PCTypedEventArgs<T>> TypedHandler { get; protected set; }
        public Action<T, T> TypedDoWhenChanged { get; protected set; }

        public CallPcTypedEventSubscriberDelegate<T> TypedHandlerProxy { get; protected set; }

        #endregion

        #region ISubscription Implementation

        public ExKeyT OwnerPropId { get; protected set; }
        public Type PropertyType => typeof(T);

        public SubscriptionKind SubscriptionKind { get; protected set; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; protected set; }
        //public SubscriptionTargetKind SubscriptionTargetKind { get; protected set; }

        public WeakReference Target { get; protected set; }
        public string MethodName { get; protected set; }

        //public EventHandlerProxy<PCGenEventArgs> GenHandlerProxy => null;
        //public PCObjectEventAction ObjHandlerProxy { get; protected set; }
        //public EventHandlerProxy<PropertyChangedEventArgs> StandardHandlerProxy => null;

        public Delegate HandlerProxy { get; protected set; }
        //public CallPCObjEventSubscriberDelegate ObjHandlerProxy { get; }

        public EventHandler<PcGenEventArgs> GenHandler { get; protected set; }
        //public EventHandler<PCObjectEventArgs> ObjHandler { get; protected set; }
        public EventHandler<PropertyChangedEventArgs> StandardHandler { get; protected set; }

        public Action<object, object> GenDoWhenChanged => null;
        public Action Action => null;

        // Binding Subscription Members
        public LocalBindingInfo BindingInfo { get; protected set; }
        public object LocalBinderAsObject => null;

        #endregion
    }
}
