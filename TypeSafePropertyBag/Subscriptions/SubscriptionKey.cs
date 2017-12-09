﻿using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class SubscriptionKey<T> : SubscriptionKeyGen, ISubscriptionKey<T>, IEquatable<SubscriptionKey<T>>
    {
        #region Public Properties

        public EventHandler<PCTypedEventArgs<T>> TypedHandler { get; private set; }
        public Action<T, T> TypedDoWhenChanged { get; private set; }

        #endregion

        #region Constructors

        // Typed Handler -- PCTypeEventArgs<T>
        public SubscriptionKey
            (
            ExKeyT exKey,
            EventHandler<PCTypedEventArgs<T>> handler,
            SubscriptionPriorityGroup subscriptionPriorityGroup,
            bool keepRef
            )
            : base(exKey, target: handler.Target, method: handler.Method, kind: SubscriptionKind.TypedHandler,
                  subscriptionPriorityGroup: subscriptionPriorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        {
            TypedHandler = handler;
        }

        // Gen Handler -- PCGenEventArgs
        public SubscriptionKey(SimpleExKey exKey, EventHandler<PcGenEventArgs> handler, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, handler, subscriptionPriorityGroup, keepRef: keepRef)
        {
        }

        // Obj Handler -- PCObjEventArgs
        public SubscriptionKey(SimpleExKey exKey, EventHandler<PcObjectEventArgs> handler, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, handler, subscriptionPriorityGroup, keepRef: keepRef)
        {
        }

        // Standard Handler -- PropertyChangedEventArgs
        public SubscriptionKey(SimpleExKey exKey, EventHandler<PropertyChangedEventArgs> handler, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, handler, subscriptionPriorityGroup, keepRef)
        {
        }

        // Typed Action
        public SubscriptionKey(SimpleExKey exKey, Action<T, T> action, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, target: action.Target, method: action.Method, kind: SubscriptionKind.TypedAction,
                  subscriptionPriorityGroup: subscriptionPriorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        {
        }

        // Action No Parameters / i.e., Message
        public SubscriptionKey(SimpleExKey exKey, Action action, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, action, subscriptionPriorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        {
        }

        // Action<object, object>
        public SubscriptionKey(SimpleExKey exKey, Action <object, object> genAction, SubscriptionPriorityGroup subscriptionPriorityGroup, bool keepRef)
            : base(exKey, genAction, subscriptionPriorityGroup, keepRef: keepRef, subscriptionFactory: CreateSubscriptionGen)
        {
        }

        #endregion

        #region Public Methods

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

        new public static ISubscription CreateSubscriptionGen(ISubscriptionKeyGen subscriptionRequestGen)
        {
            return (ISubscription)CreateSubscription((ISubscriptionKey<T>)subscriptionRequestGen);
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
