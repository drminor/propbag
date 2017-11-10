using DRM.TypeSafePropertyBag.Fundamentals;
using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class BindingSubscription<T> : AbstractSubscripton<T>
    {
        #region ISubscription<T> implementation

        new public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        //public Action<T, T> TypedDoWhenChanged { get; }

        #endregion

        #region ISubscription Implementation

        //public IExplodedKey<ulong, uint, uint> ExKey { get; } // This identifies the source of the binding


        new public EventHandler<PCGenEventArgs> GenHandler => null;
        new public EventHandler<PropertyChangedEventArgs> StandardHandler => null;

        new public Action<object, object> GenDoWhenChanged => null;
        new public Action Action => null;

        #endregion

        #region Constructors

        public BindingSubscription(ISubscriptionKey<T> sKey)
        {
            SourcePropId = sKey.ExKey;
            TypedDoWhenChanged = sKey.TypedDoWhenChanged;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            SubscriptionTargetKind = sKey.SubscriptionTargetKind;
        }

        #endregion
    }
}
