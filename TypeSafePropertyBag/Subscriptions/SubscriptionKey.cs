using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt32, UInt32>;

    public class SubscriptionKey<T> : SubscriptionKeyGen, ISubscriptionKey<T>
    {
        public EventHandler<PCTypedEventArgs<T>> TypedHandler { get; private set; }
        public Action<T, T> TypedDoWhenChanged { get; private set; }

        #region Constructors

        // Typed Handler -- PCTypeEventArgs<T>
        public SubscriptionKey
            (
            ExKeyT exKey,
            //IPropBag host,
            //uint propId,
            //IPropStoreAccessService<PropIdType, PropNameType> storeAccessor,
            EventHandler<PCTypedEventArgs<T>> handler,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            bool keepRef
            )
            : base(exKey, target: handler.Target, method: handler.Method, kind: SubscriptionKind.TypedHandler,
                  subscriptionPriorityGroup: subscriptionPriorityGroup, keepRef: keepRef, subscriptionCreator: CreateSubscriptionGen)
        {
            TypedHandler = handler;
        }

        private static SimpleExKey GetTheKey(IPropBag host, uint propId, IPropStoreAccessService<PropIdType, PropNameType> storeAccessor)
        {
            SimpleExKey result = ((IHaveTheSimpleKey)storeAccessor).GetTheKey(host, propId);
            return result;
        }

        // Gen Handler -- PCGenEventArgs
        public SubscriptionKey(SimpleExKey exKey, EventHandler<PCGenEventArgs> handler, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, handler, subscriptionPriorityGroup, keepRef: keepRef, subscriptionCreator: CreateSubscriptionGen)
        {
        }

        // Standard Handler -- PropertyChangedEventArgs
        public SubscriptionKey(SimpleExKey exKey, EventHandler<PropertyChangedEventArgs> handler, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, handler, subscriptionPriorityGroup, keepRef, subscriptionCreator: CreateSubscriptionGen)
        {
        }

        // Typed Action
        public SubscriptionKey(SimpleExKey exKey, Action<T, T> action, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, target: action.Target, method: action.Method, kind: SubscriptionKind.TypedAction,
                  subscriptionPriorityGroup: subscriptionPriorityGroup, keepRef: keepRef, subscriptionCreator: CreateSubscriptionGen)
        {
        }

        // Action No Parameters / i.e., Message
        public SubscriptionKey(SimpleExKey exKey, Action action, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, action, subscriptionPriorityGroup, keepRef: keepRef, subscriptionCreator: CreateSubscriptionGen)
        {
        }

        // Action<object, object>
        public SubscriptionKey(SimpleExKey exKey, Action <object, object> genAction, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, genAction, subscriptionPriorityGroup, keepRef: keepRef, subscriptionCreator: CreateSubscriptionGen)
        {
        }

        #endregion

        public new void MarkAsUsed()
        {
            TypedHandler = null;
            TypedDoWhenChanged = null;

            base.MarkAsUsed();
        }

        public static ISubscription<T> CreateSubscription(ISubscriptionKey<T> subscriptionRequest)
        {
            ISubscription<T> result = new Subscription<T>(subscriptionRequest);
            subscriptionRequest.MarkAsUsed();

            return result;
        }

        public static ISubscriptionGen CreateSubscriptionGen(ISubscriptionKeyGen subscriptionRequestGen)
        {
            return (ISubscriptionGen)CreateSubscription((ISubscriptionKey<T>)subscriptionRequestGen);
        }

    }
}
