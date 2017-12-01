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
    using System.Collections.Generic;

    public class BindingSubscription<T> : AbstractSubscripton<T>, IBindingSubscription<T>, IEquatable<BindingSubscription<T>>, IEquatable<ISubscriptionGen>
    {
        #region IBindingSubscription<T> Implementation

        public LocalBinder<T> LocalBinder { get; }

        #endregion

        #region ISubscription<T> Implementation

        new public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        new public Action<T, T> TypedDoWhenChanged => null;

        #endregion

        #region ISubscription Implementation

        new public ExKeyT SourcePropRef => null;

        new public EventHandler<PCGenEventArgs> GenHandler => null;
        new public EventHandler<PCObjectEventArgs> ObjHandler => null;
        new public EventHandler<PropertyChangedEventArgs> StandardHandler => null;

        new public Action<object, object> GenDoWhenChanged => null;
        new public Action Action => null;

        new public object Target => null;
        new public MethodInfo Method => null;

        new public object LocalBinderRefProxy => (object)LocalBinder;

        #endregion

        #region Constructors

        public BindingSubscription(IBindingSubscriptionKey<T> sKey, PSAccessServiceType propStoreAccessService)
        {
            TargetPropRef = sKey.TargetPropRef;
            BindingInfo = sKey.BindingInfo;

            SubscriptionKind = sKey.SubscriptionKind;
            SubscriptionPriorityGroup = sKey.SubscriptionPriorityGroup;
            SubscriptionTargetKind = sKey.SubscriptionTargetKind;

            LocalBinder = new LocalBinder<T>(propStoreAccessService, TargetPropRef, sKey.BindingInfo);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BindingSubscription<T>);
        }

        public bool Equals(BindingSubscription<T> other)
        {
            return other != null && object.ReferenceEquals(LocalBinder, other.LocalBinder);
        }

        public override int GetHashCode()
        {
            return LocalBinder.GetHashCode();
        }

        public bool Equals(ISubscriptionGen other)
        {
            return other != null && object.ReferenceEquals(LocalBinder, other.LocalBinderRefProxy);
        }

        public static bool operator ==(BindingSubscription<T> subscription1, BindingSubscription<T> subscription2)
        {
            return object.ReferenceEquals(subscription1.LocalBinder, subscription2.LocalBinder);
        }

        public static bool operator !=(BindingSubscription<T> subscription1, BindingSubscription<T> subscription2)
        {
            return !(subscription1 == subscription2);
        }

        #endregion
    }
}
