using System;

namespace DRM.TypeSafePropertyBag
{
    using PropIdType = UInt32;
    using PropNameType = String;

    public class BindingSubscriptionKey<T> : SubscriptionKeyGen, IBindingSubscriptionKey<T>
    {
        #region IBindingSubscription<T> implementation

        // Also see SubscriptionKeyGen for properties that do not require a type parameter.

        #endregion

        #region Subscription<T> implementation

        public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        public Action<T, T> TypedDoWhenChanged { get; private set; }

        #endregion

        #region Constructors

        //// Typed Action
        //public BindingSubscriptionKey
        //    (
        //        SimpleExKey sourcePropId,
        //        SimpleExKey targetPropId,
        //        LocalBindingInfo bindingInfo,
        //        Action<T, T> action
        //    )
        //    : base
        //    (
        //        sourcePropId,
        //        target: action.Target,
        //        method: action.Method,
        //        kind: SubscriptionKind.LocalBinding,
        //        subscriptionPriorityGroup: SubscriptionPriorityGroup.First,
        //        keepRef: false,
        //        subscriptionCreator: CreateSubscriptionGen
        //    )
        //{
        //    TypedDoWhenChanged = action;
        //    TargetPropId = targetPropId;
        //    BindingInfo = bindingInfo;
        //}

        public BindingSubscriptionKey
            (
                IPropBag targetHost,
                PropIdType propId,
                IPropStoreAccessService<PropIdType, PropNameType> storeAccessor,
                LocalBindingInfo bindingInfo
            )
            : base
            (
                GetTheKey(targetHost, propId, storeAccessor),
                bindingInfo, 
                SubscriptionKind.LocalBinding,
                SubscriptionPriorityGroup.First,
                CreateSubscriptionGen
            )
        {
            TypedDoWhenChanged = null;
        }


        //kind: SubscriptionKind.LocalBinding,
        //        subscriptionPriorityGroup: SubscriptionPriorityGroup.First,
        //        keepRef: false,
        //        subscriptionCreator: CreateSubscriptionGen

        private static SimpleExKey GetTheKey(IPropBag host, uint propId, IPropStoreAccessService<PropIdType, PropNameType> storeAccessor)
        {
            SimpleExKey result = ((IHaveTheSimpleKey)storeAccessor).GetTheKey(host, propId);
            return result;
        }

        #endregion

        public new void MarkAsUsed()
        {
            //TypedHandler = null;
            TypedDoWhenChanged = null;

            base.MarkAsUsed();
        }

        public static IBindingSubscription<T> CreateSubscription(IBindingSubscriptionKey<T> bindingRequest)
        {
            IBindingSubscription<T> result = new BindingSubscription<T>(bindingRequest);
            bindingRequest.MarkAsUsed();

            return result;
        }

        public static ISubscriptionGen CreateSubscriptionGen(ISubscriptionKeyGen bindingRequestGen)
        {
            return (ISubscriptionGen)CreateSubscription((IBindingSubscriptionKey<T>)bindingRequestGen);
        }

    }
}
