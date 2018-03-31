using System;

namespace DRM.TypeSafePropertyBag
{
    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;
    using PSAccessServiceInterface = IPropStoreAccessService<UInt32, String>;
    using System.Collections.Generic;

    public class BindingSubscriptionKey<T> : SubscriptionKeyGen, IBindingSubscriptionKey<T>, IEquatable<BindingSubscriptionKey<T>>
    {
        #region IBindingSubscription<T> implementation

        // Also see SubscriptionKeyGen for properties that do not require a type parameter.

        #endregion

        #region Subscription<T> implementation

        public EventHandler<PcTypedEventArgs<T>> TypedHandler => null;
        public Action<T, T> TypedDoWhenChanged => null;

        #endregion

        #region Constructors

        public BindingSubscriptionKey
            (
                ExKeyT ownerPropId,
                LocalBindingInfo bindingInfo
            )
            : base
            (
                ownerPropId,
                typeof(T),
                bindingInfo,
                SubscriptionKind.LocalBinding,
                SubscriptionPriorityGroup.First,
                CreateBindingGen
            )
        {
        }

        #endregion

        public new void MarkAsUsed()
        {
            base.MarkAsUsed();
        }

        public static IBindingSubscription<T> CreateBinding(IBindingSubscriptionKey<T> bindingRequest, PSAccessServiceInterface propStoreAccessService)
        {
            IBindingSubscription<T> result = new BindingSubscription<T>(bindingRequest, propStoreAccessService);
            bindingRequest.MarkAsUsed();

            return result;
        }

        public static ISubscription CreateBindingGen(ISubscriptionKeyGen bindingRequestGen, PSAccessServiceInterface propStoreAccessService)
        {
            return (ISubscription)CreateBinding((IBindingSubscriptionKey<T>)bindingRequestGen, propStoreAccessService);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BindingSubscriptionKey<T>);
        }

        public bool Equals(BindingSubscriptionKey<T> other)
        {
            return other != null &&
                OwnerPropId == other.OwnerPropId
                && BindingInfo == other.BindingInfo;
        }

        public override int GetHashCode()
        {
            var hashCode = -748789155;
            hashCode = hashCode * -1521134295 + OwnerPropId.GetHashCode();
            hashCode = hashCode * -1521134295 + BindingInfo.GetHashCode();
            return hashCode;
        }

        // TODO: Does this call our Equals implementation?
        public static bool operator ==(BindingSubscriptionKey<T> key1, BindingSubscriptionKey<T> key2)
        {
            return EqualityComparer<BindingSubscriptionKey<T>>.Default.Equals(key1, key2);
        }

        public static bool operator !=(BindingSubscriptionKey<T> key1, BindingSubscriptionKey<T> key2)
        {
            return !(key1 == key2);
        }
    }
}
