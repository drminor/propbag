﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    public class AbstractSubscripton<T> : ISubscription<T> 
    {
        #region ISubscription<T> implementation

        public EventHandler<PCTypedEventArgs<T>> TypedHandler { get; protected set; }
        public Action<T, T> TypedDoWhenChanged { get; protected set; }

        #endregion

        #region ISubscription Implementation

        public ExKeyT SourcePropId { get; protected set; }

        public SubscriptionKind SubscriptionKind { get; protected set; }
        public SubscriptionTargetKind SubscriptionTargetKind { get; protected set; }
        public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; protected set; }

        public Type PropertyType => typeof(T);

        public EventHandler<PCGenEventArgs> GenHandler { get; protected set; }
        public EventHandler<PropertyChangedEventArgs> StandardHandler { get; protected set; }

        public Action<object, object> GenDoWhenChanged { get; protected set; }
        public Action Action { get; protected set; }

        public object Target { get; protected set; }
        public MethodInfo Method { get; protected set; }

        // Binding Subscription Members
        public ExKeyT TargetPropId { get; protected set; }
        public LocalBindingInfo BindingInfo { get; protected set; }

        #endregion

        #region Constructors

        #endregion
    }
}