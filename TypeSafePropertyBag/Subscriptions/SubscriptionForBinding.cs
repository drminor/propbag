using System;
using System.ComponentModel;

namespace DRM.TypeSafePropertyBag
{
    public class BindingSubscription<T> : AbstractSubscripton<T>, IBindingSubscription<T>
    {
        #region IBindingSubscription<T> Implementation

        public SimpleExKey TargetPropId { get; }
        public LocalBindingInfo BindingInfo { get; }

        #endregion

        #region ISubscription<T> Implementation

        new public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        //public Action<T, T> TypedDoWhenChanged { get; }

        #endregion

        #region ISubscription Implementation

        //public IExplodedKey<ulong, uint, uint> SourcePropId { get; protected set; }

        new public EventHandler<PCGenEventArgs> GenHandler => null;
        new public EventHandler<PropertyChangedEventArgs> StandardHandler => null;

        new public Action<object, object> GenDoWhenChanged => null;
        new public Action Action => null;



        #endregion

        #region Constructors

        public BindingSubscription(IBindingSubscriptionKey<T> sKey)
        {
            SourcePropId = sKey.SourcePropRef;
            TypedDoWhenChanged = sKey.TypedDoWhenChanged;

            TargetPropId = sKey.TargetPropRef;
            BindingInfo = sKey.BindingInfo;

            Target = sKey.Target;
            Method = sKey.Method;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            SubscriptionTargetKind = sKey.SubscriptionTargetKind;
        }

        #endregion
    }
}
