﻿using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.ComponentModel;
using System.Reflection;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The type of the property to which this subscription will subscribe.</typeparam>
    public interface IBindingSubscription<T> : ISubscription<T>
    {
        IExplodedKey<ulong, uint, uint> TargetPropId { get; }
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

    public interface ISubscriptionGen
    {
        IExplodedKey<ulong, uint, uint> SourcePropId { get; }
        Type PropertyType { get; }

        SubscriptionKind SubscriptionKind { get; }
        SubscriptionTargetKind SubscriptionTargetKind { get; }
        SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        EventHandler<PCGenEventArgs> GenHandler { get; }
        EventHandler<PropertyChangedEventArgs> StandardHandler { get; }
        Action<object, object> GenDoWhenChanged { get; }
        Action Action { get; }
    }

}
