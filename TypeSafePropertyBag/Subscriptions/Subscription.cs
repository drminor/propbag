using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class Subscription<T> : AbstractSubscripton<T>
    {
        #region ISubscription<T> implementation

        //public EventHandler<PCTypedEventArgs<T>> TypedHandler { get; }
        //public Action<T, T> TypedDoWhenChanged { get; }

        #endregion

        #region ISubscription Implementation

        //public IExplodedKey<ulong, uint, uint> ExKey { get; }

        //public SubscriptionKind SubscriptionKind { get; }
        //public SubscriptionTargetKind SubscriptionTargetKind { get; }
        //public SubscriptionPriorityGroup SubscriptionPriorityGroup { get; }

        //public Type PropertyType => typeof(T);

        //public EventHandler<PCGenEventArgs> GenHandler { get; }
        //public EventHandler<PropertyChangedEventArgs> StandardHandler { get; }

        //public Action<object, object> GenDoWhenChanged { get; }
        //public Action Action { get; }

        #endregion

        #region Constructors

        // TODO: Consider adding additional constructors, very similar to the ones on SubscriptionKey.
        public Subscription(ISubscriptionKey<T> sKey)
        {
            SourcePropId = sKey.ExKey;
            TypedHandler = sKey.TypedHandler;
            GenHandler = sKey.GenHandler;
            StandardHandler = sKey.StandardHandler;
            TypedDoWhenChanged = sKey.TypedDoWhenChanged;
            GenDoWhenChanged = sKey.GenDoWhenChanged;
            Action = sKey.Action;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            SubscriptionTargetKind = sKey.SubscriptionTargetKind;
        }

        #endregion
    }
}
