using DRM.TypeSafePropertyBag.Fundamentals;
using System;

namespace DRM.TypeSafePropertyBag.EventManagement
{
    public class BindingSubscriptionKey<T> : SubscriptionKeyGen, IBindingSubscriptionKey<T>
    {
        #region IBindingSubscription<T> implementation

        public SimpleExKey TargetPropId { get; }
        public LocalBindingInfo BindingInfo { get; }

        #endregion

        #region Subscription<T> implementation

        public EventHandler<PCTypedEventArgs<T>> TypedHandler => null;
        public Action<T, T> TypedDoWhenChanged { get; private set; }

        #endregion

        #region Constructors

        // Typed Action
        public BindingSubscriptionKey
            (
                SimpleExKey sourcePropId,
                SimpleExKey targetPropId,
                LocalBindingInfo bindingInfo,
                Action<T, T> action
            )
            : base
            (
                sourcePropId,
                target: action.Target,
                method: action.Method,
                kind: SubscriptionKind.LocalBinding,
                subscriptionPriorityGroup: SubscriptionPriorityGroup.First,
                keepRef: false,
                subscriptionCreator: CreateSubscriptionGen
            )
        {
            TypedDoWhenChanged = action;
            TargetPropId = targetPropId;
            BindingInfo = bindingInfo;
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
