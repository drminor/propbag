using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class SubscriptionKey<T> : SubscriptionKeyGen, ISubscriptionKey<T>, IEquatable<SubscriptionKey<T>>
    {
        #region Public Properties

        public EventHandler<PcTypedEventArgs<T>> TypedHandler { get; private set; }
        public Action<T, T> TypedDoWhenChanged { get; private set; }

        #endregion

        #region Constructors

        // Typed Handler -- PCTypeEventArgs<T>
        public SubscriptionKey
            (
            ExKeyT exKey,
            EventHandler<PcTypedEventArgs<T>> handler,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            bool keepRef
            )
            : base(exKey, target: handler.Target, method: handler.Method, kind: SubscriptionKind.TypedHandler,
                  subscriptionPriorityGroup: subscriptionPriorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        {
            TypedHandler = handler;
        }

        public SubscriptionKey
            (
            ExKeyT exKey,
            object target,
            MethodInfo methodInfo,
            SubscriptionPriorityGroup priorityGroup,
            bool keepRef
            )
            : base(exKey, target: target, method: methodInfo, kind: SubscriptionKind.TypedHandler,
          subscriptionPriorityGroup: priorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        {
            TypedHandler = null;
        }

        //// Typed Action
        //public SubscriptionKey(SimpleExKey exKey, Action<T, T> action, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
        //    : base(exKey, target: action.Target, method: action.Method, kind: SubscriptionKind.TypedAction,
        //          subscriptionPriorityGroup: subscriptionPriorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        //{
        //}

        #endregion

        #region Public Methods

        public new void MarkAsUsed()
        {
            TypedHandler = null;
            TypedDoWhenChanged = null;

            base.MarkAsUsed();
        }

        public static ISubscription<T> CreateSubscription(ISubscriptionKey<T> subscriptionRequest, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            ISubscription<T> result = new Subscription<T>(subscriptionRequest, handlerDispatchDelegateCacheProvider);
            subscriptionRequest.MarkAsUsed();

            return result;
        }

        new public static ISubscription CreateSubscriptionGen(ISubscriptionKeyGen subscriptionRequestGen, IProvideHandlerDispatchDelegateCaches handlerDispatchDelegateCacheProvider)
        {
            return (ISubscription)CreateSubscription((ISubscriptionKey<T>)subscriptionRequestGen, handlerDispatchDelegateCacheProvider);
        }

        #endregion

        #region IEquatable Support and Object Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscriptionKeyGen);
        }

        public bool Equals(SubscriptionKey<T> other)
        {
            return other != null && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SubscriptionKey<T> key1, SubscriptionKey<T> key2)
        {
            return ((SubscriptionKeyGen)key1) == (SubscriptionKeyGen)key2;
        }

        public static bool operator !=(SubscriptionKey<T> key1, SubscriptionKey<T> key2)
        {
            return !(key1 == key2);
        }

        #endregion
    }
}
