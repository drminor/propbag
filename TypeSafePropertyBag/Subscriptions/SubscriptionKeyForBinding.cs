using System;

namespace DRM.TypeSafePropertyBag
{
    using CompositeKeyType = UInt64;
    using ObjectIdType = UInt64;

    using PropIdType = UInt32;
    using PropNameType = String;

    using ExKeyT = IExplodedKey<UInt64, UInt64, UInt32>;

    using PSAccessServiceType = IPropStoreAccessService<UInt32, String>;

    public class BindingSubscriptionKey<T> : SubscriptionKeyGen, IBindingSubscriptionKey<T>
    {
        #region IBindingSubscription<T> implementation

        // Also see SubscriptionKeyGen for properties that do not require a type parameter.

        #endregion

        #region Subscription<T> implementation

        public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        public Action<T, T> TypedDoWhenChanged => null;

        #endregion

        #region Constructors

        public BindingSubscriptionKey
            (
                ExKeyT targetPropRef,
                LocalBindingInfo bindingInfo
            )
            : base
            (
                targetPropRef,
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

        public static IBindingSubscription<T> CreateBinding(IBindingSubscriptionKey<T> bindingRequest, PSAccessServiceType propStoreAccessService)
        {
            IBindingSubscription<T> result = new BindingSubscription<T>(bindingRequest, propStoreAccessService);
            bindingRequest.MarkAsUsed();

            return result;
        }

        public static ISubscriptionGen CreateBindingGen(ISubscriptionKeyGen bindingRequestGen, PSAccessServiceType propStoreAccessService)
        {
            return (ISubscriptionGen)CreateBinding((IBindingSubscriptionKey<T>)bindingRequestGen, propStoreAccessService);
        }

    }
}
