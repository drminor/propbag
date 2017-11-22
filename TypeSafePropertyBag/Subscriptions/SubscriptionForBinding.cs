using System;
using System.ComponentModel;

using DRM.TypeSafePropertyBag.LocalBinding;
using System.Reflection;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    public class BindingSubscription<T> : AbstractSubscripton<T>, IBindingSubscription<T>
    {
        #region IBindingSubscription<T> Implementation

        public LocalBinder<T> LocalBinder { get; }

        #endregion

        #region ISubscription<T> Implementation

        new public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        new public Action<T, T> TypedDoWhenChanged => null;

        #endregion

        #region ISubscription Implementation

        new public ExKeyT SourcePropId => null;

        new public EventHandler<PCGenEventArgs> GenHandler => null;
        new public EventHandler<PropertyChangedEventArgs> StandardHandler => null;

        new public Action<object, object> GenDoWhenChanged => null;
        new public Action Action => null;

        new public object Target => null;
        new public MethodInfo Method => null;

        #endregion

        #region Constructors

        public BindingSubscription(IBindingSubscriptionKey<T> sKey, PSAccessServiceType propStoreAccessService)
        {
            TargetPropId = sKey.TargetPropRef;
            BindingInfo = sKey.BindingInfo;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            SubscriptionTargetKind = sKey.SubscriptionTargetKind;

            LocalBinder = new LocalBinder<T>(propStoreAccessService, TargetPropId, sKey.BindingInfo);
        }

        #endregion
    }
}
